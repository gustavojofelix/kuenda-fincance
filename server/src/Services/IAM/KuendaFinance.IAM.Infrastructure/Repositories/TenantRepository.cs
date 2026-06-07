using System;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.IAM.Domain.Entities;
using KuendaFinance.IAM.Domain.Repositories;
using KuendaFinance.IAM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KuendaFinance.IAM.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly IamDbContext _context;

    public TenantRepository(IamDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(tenant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.ToLowerInvariant().Trim();
        return await _context.Tenants.AnyAsync(t => t.Code == normalizedCode, cancellationToken);
    }

    public async Task<bool> SubdomainExistsAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        var normalizedSubdomain = subdomain.ToLowerInvariant().Trim();
        return await _context.Tenants.AnyAsync(t => t.Subdomain == normalizedSubdomain, cancellationToken);
    }

    public async Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.ToLowerInvariant().Trim();
        return await _context.Tenants.FirstOrDefaultAsync(t => t.Code == normalizedCode, cancellationToken);
    }

    public async Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        var normalizedSubdomain = subdomain.ToLowerInvariant().Trim();
        return await _context.Tenants.FirstOrDefaultAsync(t => t.Subdomain == normalizedSubdomain, cancellationToken);
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
