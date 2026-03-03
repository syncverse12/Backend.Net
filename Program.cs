using FluentValidation;
using FluentValidation.AspNetCore;
using SyncVerse.API.Authorization.Handlers;
using SyncVerse.API.Authorization.Requirements;
using SyncVerse.API.Hubs;
using SyncVerse.API.JwtFeatuers;
using SyncVerse.API.Middleware;
using SyncVerse.Application.Interfaces;
using SyncVerse.Application.Interfaces.Identity;
using SyncVerse.Application.Interfaces.Notifications;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Application.Interfaces.Task.Manager;
using SyncVerse.Application.Interfaces.Tasks.Comments;
using SyncVerse.Application.Interfaces.Tasks.TimeTracking;
using SyncVerse.Application.Services.Identity;
using SyncVerse.Application.Services.Milestones;
using SyncVerse.Application.Services.Notifications;
using SyncVerse.Application.Services.Project.Employee;
using SyncVerse.Application.Services.Task.Manager;
using SyncVerse.Application.Services.Tasks.Comments;
using SyncVerse.Application.Services.Tasks.TimeTracking;
using SyncVerse.Domain.Entities;
using SyncVerse.Infrastructure.Data;
using SyncVerse.Infrastructure.Persistence;
using SyncVerse.Infrastructure.Persistence.Repositories;
using SyncVerse.Infrastructure.Realtime;
using SyncVerse.Infrastructure.SeedConfiguration;
using SyncVerse.Infrastructure.Services.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// --- 1. Register Services ---
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddDbContext<DatabaseDbContext>(opts =>
        opts.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<DatabaseDbContext>()
    .AddDefaultTokenProviders();

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var securityKey = jwtSettings["securityKey"] ?? throw new InvalidOperationException("JWT securityKey is missing in appsettings.json");

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["validIssuer"],
        ValidAudience = jwtSettings["validAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey))
    };
    
    // ✅ SignalR authentication
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }
            
            return System.Threading.Tasks.Task.CompletedTask;
        }
    };
});

// ✅ Services Registration
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ICategoryTaskService, CategoryTaskService>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IInvitationService, MockInvitationService>();
builder.Services.AddScoped<IMilestoneService, MilestoneService>();
builder.Services.AddScoped<ITaskCommentService, TaskCommentService>();
builder.Services.AddScoped<ITimeLogService, TimeLogService>();
builder.Services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();  
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmployeeProjectService, EmployeeProjectService>();

builder.Services.AddScoped<IAuthorizationHandler, ManagerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TaskOwnerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ReviewTaskAuthorizationHandler>();

// SignalR
builder.Services.AddSignalR();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManagerOnly", policy =>
        policy.Requirements.Add(new ManagerRequirement()));

    options.AddPolicy("TaskOwner", policy =>
        policy.Requirements.Add(new TaskOwnerRequirement()));

    options.AddPolicy("ReviewTask", policy =>
        policy.Requirements.Add(new ReviewTaskRequirement()));
        
    options.AddPolicy("EmployeeOnly", policy =>
        policy.RequireRole("Employee"));
});



// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<IAuthService>();
builder.Services.AddFluentValidationClientsideAdapters();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Project Management API", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

// --- 2. Middleware Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

// --- 3. Database Seeding ---
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<DatabaseDbContext>();

        // ✅ تطبيق الـ migrations بدلاً من EnsureCreated
        await context.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

        await DefaultAdminSeeder.SeedAsync(userManager, roleManager);

        Console.WriteLine("✅ Database seeding completed successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error during database seeding: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        // Don't throw - let the app continue to run
    }
}

await app.RunAsync();