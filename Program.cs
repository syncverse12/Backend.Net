using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SyncVerse.API.Authorization.Handlers;
using SyncVerse.API.Authorization.Requirements;
using SyncVerse.API.Hubs;
using SyncVerse.API.JwtFeatuers;
using SyncVerse.API.Middleware;
using SyncVerse.Application.Interfaces;
using SyncVerse.Application.Interfaces.Attachments;
using SyncVerse.Application.Interfaces.Identity;
using SyncVerse.Application.Interfaces.Notifications;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Application.Interfaces.Profile;
using SyncVerse.Application.Interfaces.Storage;
using SyncVerse.Application.Interfaces.Task.Manager;
using SyncVerse.Application.Interfaces.Tasks.Comments;
using SyncVerse.Application.Interfaces.Tasks.Employee;
using SyncVerse.Application.Interfaces.Tasks.TimeTracking;
using SyncVerse.Application.Interfaces.Team; 
using SyncVerse.Application.Interfaces.WorkspaceInvitation; 
using SyncVerse.Application.Services.Attachments;
using SyncVerse.Application.Services.Identity;
using SyncVerse.Application.Services.Milestones;
using SyncVerse.Application.Services.Notifications;
using SyncVerse.Application.Services.Profile;
using SyncVerse.Application.Services.Project.Employee;
using SyncVerse.Application.Services.Task.Employee;
using SyncVerse.Application.Services.Task.Manager;
using SyncVerse.Application.Services.Tasks.Comments;
using SyncVerse.Application.Services.Tasks.TimeTracking;
using SyncVerse.Application.Services.Team;
using SyncVerse.Application.Services.WorkspaceInvitation; 
using SyncVerse.Domain.Entities;
using SyncVerse.Domain.Enums;
using SyncVerse.Infrastructure.Data;
using SyncVerse.Infrastructure.Persistence;
using SyncVerse.Infrastructure.Persistence.Repositories;
using SyncVerse.Infrastructure.Realtime;
using SyncVerse.Infrastructure.SeedConfiguration;
using SyncVerse.Infrastructure.Services.Email;
using SyncVerse.Infrastructure.Storage;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Register Services ---
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddDbContext<DatabaseDbContext>(opts =>
        opts.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<User, Role>(options =>
{
    // Password settings 
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    
    // SignIn settings
    options.SignIn.RequireConfirmedEmail = false; // للتطوير
})
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

// ✅ CORS Configuration - Allow Frontend, Flutter, Unity
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",      // React/Next.js Dev
                "http://localhost:4200",      // Angular Dev
                "http://localhost:8080",      // Vue.js Dev
                "http://localhost:5173",      // Vite Dev
                "https://yourdomain.com",     // Production Frontend
                "capacitor://localhost",      // Capacitor/Flutter
                "ionic://localhost"           // Ionic
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition"); // For file downloads
    });
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
builder.Services.AddScoped<IMilestoneService, MilestoneService>();
builder.Services.AddScoped<ITaskCommentService, TaskCommentService>();
builder.Services.AddScoped<ITimeLogService, TimeLogService>();
builder.Services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();  
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmployeeProjectService, EmployeeProjectService>();
builder.Services.AddScoped<IEmployeeTaskService, EmployeeTaskService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ICompanyInvitationService, CompanyInvitationService>(); 

builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IProjectAttachmentService, ProjectAttachmentService>();
builder.Services.AddScoped<ITaskAttachmentService, TaskAttachmentService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

builder.Services.AddScoped<IAuthorizationHandler, ManagerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TaskOwnerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ReviewTaskAuthorizationHandler>();

builder.Services.AddSignalR();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin", "HR"));

    options.AddPolicy("ProjectManagerOnly", policy =>
        policy.RequireRole("ProjectManager", "Admin"));

    options.AddPolicy("TeamLeaderOnly", policy =>
        policy.RequireRole("TeamLeader", "ProjectManager", "Admin"));

    options.AddPolicy("ManagerOnly", policy =>
        policy.Requirements.Add(new ManagerRequirement()));

    options.AddPolicy("TaskOwner", policy =>
        policy.Requirements.Add(new TaskOwnerRequirement()));

    options.AddPolicy("ReviewTask", policy =>
        policy.Requirements.Add(new ReviewTaskRequirement()));
        
    options.AddPolicy("EmployeeOnly", policy =>
        policy.RequireRole("Employee"));
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<IAuthService>();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "SyncVerse API", 
        Version = "v1",
        Description = "Project Management System API"
    });

    opt.CustomOperationIds(apiDesc =>
    {
        var controllerName = apiDesc.ActionDescriptor.RouteValues["controller"];
        var actionName = apiDesc.ActionDescriptor.RouteValues["action"];
        return $"{controllerName}_{actionName}";
    });

    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT Bearer token",
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
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
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

// ✅ CORS - Must be before Authentication/Authorization
app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "ProductionPolicy");

app.UseHttpsRedirection();
app.UseStaticFiles(); 
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
    }
}

await app.RunAsync();