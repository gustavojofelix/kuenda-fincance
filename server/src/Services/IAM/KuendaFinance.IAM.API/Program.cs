using FastEndpoints;
using FastEndpoints.Swagger;
using KuendaFinance.IAM.Application.Commands.RegisterUser;
using KuendaFinance.IAM.Domain.Authentication;
using KuendaFinance.IAM.Domain.Repositories;
using KuendaFinance.IAM.Infrastructure.Authentication;
using KuendaFinance.IAM.Infrastructure.Identity;
using KuendaFinance.IAM.Infrastructure.Persistence;
using KuendaFinance.IAM.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=kuenda_iam;Username=postgres;Password=postgres";

builder.Services.AddDbContext<IamDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Identity Configuration
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<IamDbContext>()
.AddDefaultTokenProviders();

// 3. Authentication & JWT
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "SuperSecretKeyForDevelopmentPurposes123!";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "KuendaFinance",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "KuendaFinanceClient",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});
builder.Services.AddAuthorization();

// 4. MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RegisterUserCommand>());

// 5. Dependency Injection (Application/Infrastructure)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

// 6. FastEndpoints & Swagger
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "Kuenda Finance IAM API";
        s.Version = "v1";
    };
});

var app = builder.Build();

// Auto-migrate database on startup (Development / CI)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<IamDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen(); // FastEndpoints Swagger
}

// Health-check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "IAM" }));

app.Run();
