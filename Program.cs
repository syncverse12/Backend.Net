using AutoMapper;
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
using SyncVerse.Application.Interfaces.AI;
using SyncVerse.Application.Interfaces.AI.Meeting.TaskExtraction;
using SyncVerse.Application.Interfaces.AI.Risk;
using SyncVerse.Application.Interfaces.Attachments;
using SyncVerse.Application.Interfaces.Dashboard;
using SyncVerse.Application.Interfaces.Identity;
using SyncVerse.Application.Interfaces.Meetings;
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
using SyncVerse.Application.Interfaces.Workspaces;
using SyncVerse.Application.Mapping;
using SyncVerse.Application.Services.AI;
using SyncVerse.Application.Services.Attachments;
using SyncVerse.Application.Services.Dashboard;
using SyncVerse.Application.Services.Identity;
using SyncVerse.Application.Services.Meetings;
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
using SyncVerse.Application.Services.Workspaces;
using SyncVerse.Domain.Entities;
using SyncVerse.Infrastructure.Data;
using SyncVerse.Infrastructure.Persistence;
using SyncVerse.Infrastructure.Persistence.Repositories;
using SyncVerse.Infrastructure.Realtime;
using SyncVerse.Infrastructure.SeedConfiguration;
using SyncVerse.Infrastructure.Services.Email;
using SyncVerse.Infrastructure.Storage;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var envFileName = environmentName switch
{
    "Production" => ".production.env",
    "Development" => ".dev.env",
    _ => ".local.env"
};

var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), "envs", envFileName);
LoadEnvFile(envFilePath);

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
}, typeof(MappingProfile).Assembly);

builder.Services.AddDbContext<DatabaseDbContext>(opts =>
        opts.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
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

            if (!string.IsNullOrEmpty(accessToken) && path.Value?.Contains("/hubs") == true)
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Disposition");
    });

    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "capacitor://localhost", "ionic://localhost")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition");
    });
});

// (Dependency Injection)
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
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IWorkspaceSessionService, WorkspaceSessionService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IProjectAttachmentService, ProjectAttachmentService>();
builder.Services.AddScoped<ITaskAttachmentService, TaskAttachmentService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();

builder.Services.AddScoped<IAuthorizationHandler, ManagerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TaskOwnerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ReviewTaskAuthorizationHandler>();

builder.Services.AddScoped<IAiMeetingService, AiMeetingService>();
builder.Services.AddScoped<IAiTaskExtractionService, AiTaskExtractionService>();
builder.Services.AddScoped<IAiRiskService, AiRiskService>();

builder.Services.AddSignalR();


builder.Services.AddHttpClient("AI_Transcription_Server", client =>
{
    client.BaseAddress = new Uri("https://marwaezzat8-meet.hf.space/");
    client.Timeout = TimeSpan.FromSeconds(60);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


builder.Services.AddHttpClient("AI_Meeting_Server", client =>
{
    client.BaseAddress = new Uri("https://marwaabuelkheir-meeting-summary-api.hf.space/");
    client.Timeout = TimeSpan.FromSeconds(45);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


builder.Services.AddHttpClient("AI_Task_Extraction_Server", client =>
{
    client.BaseAddress = new Uri("https://marwaabuelkheir-meeting-task-extraction-api.hf.space/");
    client.Timeout = TimeSpan.FromSeconds(45);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


builder.Services.AddHttpClient("AI_Risk_Server", client =>
{
    client.BaseAddress = new Uri("https://elkady61-risk.hf.space/");
    client.Timeout = TimeSpan.FromSeconds(60);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin", "HR"));
    options.AddPolicy("ProjectManagerOnly", policy => policy.RequireRole("ProjectManager", "Admin"));
    options.AddPolicy("TeamLeaderOnly", policy => policy.RequireRole("TeamLeader", "ProjectManager", "Admin"));
    options.AddPolicy("ManagerOnly", policy => policy.Requirements.Add(new ManagerRequirement()));
    options.AddPolicy("TaskOwner", policy => policy.Requirements.Add(new TaskOwnerRequirement()));
    options.AddPolicy("ReviewTask", policy => policy.Requirements.Add(new ReviewTaskRequirement()));
    options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("Employee"));
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<IAuthService>();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "SyncVerse API", Version = "v1" });
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
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[]{}
        }
    });


    opt.OrderActionsBy(apiDesc =>
    {
        var controllerName = apiDesc.ActionDescriptor.RouteValues["controller"];

        return controllerName switch
        {
            "Auth" => "01",
            "CompanyInvitation" => "02",
            "AiMeeting" => "03",
            "Meetings" => "04",
            _ => "99" 
        };
    });
});

var app = builder.Build();

// --- 2. الـ Middleware Pipeline ---

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("ProductionPolicy");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

// --- 3. Database Migration & Seeding ---

using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<DatabaseDbContext>();
        await context.Database.MigrateAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        await DefaultAdminSeeder.SeedAsync(userManager, roleManager);
        Console.WriteLine("✅ SyncVerse Database is ready and seeded!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Critical Error during startup: {ex.Message}");
    }
}

await app.RunAsync();

static void LoadEnvFile(string filePath)
{
    if (!File.Exists(filePath))
    {
        return;
    }

    foreach (var line in File.ReadAllLines(filePath))
    {
        var trimmedLine = line.Trim();
        if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
        {
            continue;
        }

        var separatorIndex = trimmedLine.IndexOf('=');
        if (separatorIndex <= 0)
        {
            continue;
        }

        var key = trimmedLine.Substring(0, separatorIndex).Trim();
        var value = trimmedLine.Substring(separatorIndex + 1).Trim();
        Environment.SetEnvironmentVariable(key, value);
    }
}