using System.Security.Claims;
using KuendaFinance.Operations.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.Operations.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public Guid TenantId => Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue("tenantId"), out var tenantId) 
        ? tenantId 
        : Guid.Empty;
}
