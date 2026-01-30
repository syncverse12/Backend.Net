using FluentValidation;
using FluentValidation.AspNetCore;
using Graduation_Project.API.Authorization.Handlers;
using Graduation_Project.API.Authorization.Requirements;
using Graduation_Project.API.JwtFeatuers;
using Graduation_Project.API.Middleware;
using Graduation_Project.Application.Interfaces;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Application.Interfaces.Task.Manager;
using Graduation_Project.Application.Services;
using Graduation_Project.Application.Services.Task.Manager;
using Graduation_Project.Domain.Models;
using Graduation_Project.Infrastructure.Data;
using Graduation_Project.Infrastructure.Persistence;
using Graduation_Project.Infrastructure.Persistence.Repositories;
using Graduation_Project.Infrastructure.SeedConfiguration;
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
});

//builder.Services.AddAuthorizationPolicies();


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IAuthorizationHandler, ManagerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TaskOwnerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ReviewTaskAuthorizationHandler>();


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
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
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

// --- 3. Database Seeding ---
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

    await DefaultAdminSeeder.SeedAsync(userManager, roleManager);
}

await app.RunAsync();