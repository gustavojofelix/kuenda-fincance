using System;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Domain.Entities;

namespace KuendaFinance.Operations.Domain.Repositories;

public interface ICreditSettingsRepository
{
    Task<CreditSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task SaveAsync(CreditSettings settings, CancellationToken cancellationToken = default);
}
