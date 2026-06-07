using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Domain.Entities;
using KuendaFinance.Operations.Domain.Repositories;
using KuendaFinance.Operations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KuendaFinance.Operations.Infrastructure.Repositories;

public class LoanRepository : ILoanRepository
{
    private readonly OperationsDbContext _context;

    public LoanRepository(OperationsDbContext context)
    {
        _context = context;
    }

    public async Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Loans
            .Include(l => l.Installments)
            .Include(l => l.Payments)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task AddAsync(Loan loan, CancellationToken cancellationToken = default)
    {
        await _context.Loans.AddAsync(loan, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Loan loan, CancellationToken cancellationToken = default)
    {
        _context.Loans.Update(loan);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(List<Loan> Items, int TotalCount)> GetLoansPagedAsync(
        int pageNumber,
        int pageSize,
        string status,
        Guid? clientId,
        Guid? branchId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Loans
            .Include(l => l.Installments)
            .Include(l => l.Payments)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "Todos", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(l => l.Status == status);
        }

        if (clientId.HasValue && clientId != Guid.Empty)
        {
            query = query.Where(l => l.ClientId == clientId.Value);
        }

        if (branchId.HasValue && branchId != Guid.Empty)
        {
            query = query.Where(l => l.BranchId == branchId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<Loan>> GetActiveAndLateLoansAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Loans
            .IgnoreQueryFilters()
            .Include(l => l.Installments)
            .Include(l => l.Payments)
            .Where(l => l.Status == "Active" || l.Status == "Late")
            .ToListAsync(cancellationToken);
    }
}
