using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Domain.Entities;

namespace KuendaFinance.Operations.Domain.Repositories;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Loan loan, CancellationToken cancellationToken = default);
    Task UpdateAsync(Loan loan, CancellationToken cancellationToken = default);
    Task<(List<Loan> Items, int TotalCount)> GetLoansPagedAsync(
        int pageNumber,
        int pageSize,
        string status,
        Guid? clientId,
        Guid? branchId,
        CancellationToken cancellationToken = default);

    Task<List<Loan>> GetActiveAndLateLoansAsync(CancellationToken cancellationToken = default);
}
