using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.IAM.Domain.Entities;

namespace KuendaFinance.IAM.Domain.Repositories;

public interface IUserBranchRoleRepository
{
    Task AddAsync(UserBranchRole userBranchRole, CancellationToken cancellationToken = default);
    Task<List<UserBranchRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RemoveRangeAsync(IEnumerable<UserBranchRole> userBranchRoles, CancellationToken cancellationToken = default);
}
