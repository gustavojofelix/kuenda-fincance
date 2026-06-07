using System;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.IAM.Domain.Entities;
using KuendaFinance.IAM.Domain.Repositories;
using KuendaFinance.IAM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KuendaFinance.IAM.Infrastructure.Repositories;

public class BranchRepository : IBranchRepository
{
    private readonly IamDbContext _context;

    public BranchRepository(IamDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Branch branch, CancellationToken cancellationToken = default)
    {
        await _context.Branches.AddAsync(branch, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Branches.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task UpdateAsync(Branch branch, CancellationToken cancellationToken = default)
    {
        _context.Branches.Update(branch);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Branch branch, CancellationToken cancellationToken = default)
    {
        _context.Branches.Remove(branch);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Branch>> GetBranchesByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Branches
            .Where(b => b.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }
}
