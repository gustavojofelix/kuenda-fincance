using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Domain.Entities;

namespace KuendaFinance.Operations.Domain.Repositories;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    
    Task<(List<Transaction> Items, int TotalCount)> GetTransactionsPagedAsync(
        int pageNumber,
        int pageSize,
        string searchTerm,
        string category,
        string type,
        Guid? branchId,
        CancellationToken cancellationToken = default);

    Task<decimal> GetCurrentBalanceAsync(Guid? branchId, CancellationToken cancellationToken = default);

    Task<(decimal Inflow, decimal Outflow)> GetMonthlyMetricsAsync(Guid? branchId, int year, int month, CancellationToken cancellationToken = default);
}
