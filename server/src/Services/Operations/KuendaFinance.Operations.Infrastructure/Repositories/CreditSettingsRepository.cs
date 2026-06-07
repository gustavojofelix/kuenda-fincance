using System;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Domain.Entities;
using KuendaFinance.Operations.Domain.Repositories;
using KuendaFinance.Operations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KuendaFinance.Operations.Infrastructure.Repositories;

public class CreditSettingsRepository : ICreditSettingsRepository
{
    private readonly OperationsDbContext _context;

    public CreditSettingsRepository(OperationsDbContext context)
    {
        _context = context;
    }

    public async Task<CreditSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.CreditSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);
    }

    public async Task SaveAsync(CreditSettings settings, CancellationToken cancellationToken = default)
    {
        var existing = await _context.CreditSettings
            .FirstOrDefaultAsync(s => s.TenantId == settings.TenantId, cancellationToken);

        if (existing == null)
        {
            await _context.CreditSettings.AddAsync(settings, cancellationToken);
        }
        else
        {
            existing.DefaultInterestRate = settings.DefaultInterestRate;
            existing.DefaultPenaltyRate = settings.DefaultPenaltyRate;
            existing.OriginationFee = settings.OriginationFee;
            existing.MaxTermMonths = settings.MaxTermMonths;
            existing.Currency = settings.Currency;
            _context.CreditSettings.Update(existing);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
