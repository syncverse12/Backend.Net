using FluentValidation;
using FluentValidation.AspNetCore;
using Graduation_Project.API.JwtFeatuers;
using Graduation_Project.Application.Interfaces;
using Graduation_Project.Application.Services;
using Graduation_Project.Domain.Models;
using Graduation_Project.Infrastructure.Data;
using Graduation_Project.Infrastructure.SeedConfiguration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Register Services ---
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); //  ⁄œÌ· »”Ìÿ ·÷„«‰ ⁄„· «·„«»—

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

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtHandler>();

builder.Services.AddControllers();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<IAuthService>(); // Ì›÷· «·≈‘«—… ·Ê«ÃÂ… ›Ì ÿ»ﬁ… Application

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