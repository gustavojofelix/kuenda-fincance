using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.IAM.Domain.Entities;
using KuendaFinance.IAM.Domain.Repositories;
using KuendaFinance.IAM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KuendaFinance.IAM.Infrastructure.Repositories;

public class UserBranchRoleRepository : IUserBranchRoleRepository
{
    private readonly IamDbContext _context;

    public UserBranchRoleRepository(IamDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserBranchRole userBranchRole, CancellationToken cancellationToken = default)
    {
        await _context.UserBranchRoles.AddAsync(userBranchRole, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<UserBranchRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserBranchRoles
            .Where(ubr => ubr.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}
