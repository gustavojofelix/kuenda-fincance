using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Domain.Entities;

namespace KuendaFinance.Operations.Domain.Repositories;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Client?> GetByBiAsync(string bi, Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Client client, CancellationToken cancellationToken = default);
    Task UpdateAsync(Client client, CancellationToken cancellationToken = default);
    Task<(List<Client> Items, int TotalCount)> GetClientsPagedAsync(
        int pageNumber,
        int pageSize,
        string searchTerm,
        string province,
        string status,
        Guid? branchId,
        CancellationToken cancellationToken = default);
    Task AddStatusHistoryAsync(ClientStatusHistory history, CancellationToken cancellationToken = default);
}
