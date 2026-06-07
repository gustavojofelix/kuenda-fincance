using System.Text;
using FastEndpoints;
using FastEndpoints.Swagger;
using FluentValidation;
using KuendaFinance.Operations.Application.Commands.CreateClient;
using KuendaFinance.Operations.Application.Interfaces;
using KuendaFinance.Operations.Domain.Repositories;
using KuendaFinance.Operations.Infrastructure.Persistence;
using KuendaFinance.Operations.Infrastructure.Repositories;
using KuendaFinance.Operations.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=kuenda_operations;Username=postgres;Password=postgres";

builder.Services.AddDbContext<OperationsDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Authentication & JWT
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

// 3. MediatR & Validation Pipeline
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<CreateClientCommand>();
    cfg.AddOpenBehavior(typeof(KuendaFinance.Shared.Behaviors.ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssemblyContaining<CreateClientCommand>();

// 4. Dependency Injection
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IStorageService, MinioStorageService>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICreditSettingsRepository, CreditSettingsRepository>();
builder.Services.AddScoped<KuendaFinance.Operations.Application.Interfaces.IReportingService, KuendaFinance.Operations.Infrastructure.Services.ReportingService>();
builder.Services.AddHostedService<KuendaFinance.Operations.Infrastructure.Services.DailyOverdueBackgroundService>();

// 5. FastEndpoints & Swagger
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "Kuenda Finance Operations API";
        s.Version = "v1";
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen(); // FastEndpoints Swagger
}

// Health-check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Operations" }));

app.Run();
