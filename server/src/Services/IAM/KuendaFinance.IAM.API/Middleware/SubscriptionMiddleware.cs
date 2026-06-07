using System;
using System.Security.Claims;
using System.Threading.Tasks;
using KuendaFinance.IAM.Domain.Repositories;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.IAM.API.Middleware;

public class SubscriptionMiddleware
{
    private readonly RequestDelegate _next;

    public SubscriptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantRepository tenantRepository)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip check for basic operations
        if (path.Contains("/api/auth/login") || 
            path.Contains("/api/auth/register") || 
            path.Contains("/health") || 
            path.Contains("/swagger"))
        {
            await _next(context);
            return;
        }

        var tenantIdClaim = context.User.FindFirst("tenantId")?.Value;
        if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            var tenant = await tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null || !tenant.IsActive)
            {
                context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new 
                { 
                    error = "Subscrição expirada ou suspensa.", 
                    message = "Por favor, regularize o pagamento da sua IMF para continuar a aceder ao sistema." 
                });
                return;
            }
        }

        await _next(context);
    }
}
