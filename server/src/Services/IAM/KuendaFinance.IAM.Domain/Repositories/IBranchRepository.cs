using System;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.IAM.Domain.Entities;

namespace KuendaFinance.IAM.Domain.Repositories;

public interface IBranchRepository
{
    Task AddAsync(Branch branch, CancellationToken cancellationToken = default);
    Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
