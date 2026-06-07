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

public class TransactionRepository : ITransactionRepository
{
    private readonly OperationsDbContext _context;

    public TransactionRepository(OperationsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(List<Transaction> Items, int TotalCount)> GetTransactionsPagedAsync(
        int pageNumber,
        int pageSize,
        string searchTerm,
        string category,
        string type,
        Guid? branchId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t => t.Description.ToLower().Contains(searchTerm.ToLower()) || t.Category.ToLower().Contains(searchTerm.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(category) && !string.Equals(category, "Todas", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(t => t.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(type) && !string.Equals(type, "Todas", StringComparison.OrdinalIgnoreCase) && !string.Equals(type, "Todos", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(t => t.Type == type);
        }

        if (branchId.HasValue && branchId != Guid.Empty)
        {
            query = query.Where(t => t.BranchId == branchId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<decimal> GetCurrentBalanceAsync(Guid? branchId, CancellationToken cancellationToken = default)
    {
        var baseBalance = 500000m; // Initial cash reserves/injection

        var inflows = await _context.Transactions
            .Where(t => t.Type == "Entrada" && (!branchId.HasValue || branchId == Guid.Empty || t.BranchId == branchId.Value))
            .SumAsync(t => t.Amount, cancellationToken);

        var outflows = await _context.Transactions
            .Where(t => t.Type == "Saída" && (!branchId.HasValue || branchId == Guid.Empty || t.BranchId == branchId.Value))
            .SumAsync(t => t.Amount, cancellationToken);

        return baseBalance + inflows - outflows;
    }

    public async Task<(decimal Inflow, decimal Outflow)> GetMonthlyMetricsAsync(Guid? branchId, int year, int month, CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1);

        var transactions = await _context.Transactions
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate < endDate && (!branchId.HasValue || branchId == Guid.Empty || t.BranchId == branchId.Value))
            .ToListAsync(cancellationToken);

        var inflow = transactions.Where(t => t.Type == "Entrada").Sum(t => t.Amount);
        var outflow = transactions.Where(t => t.Type == "Saída").Sum(t => t.Amount);

        return (inflow, outflow);
    }
}
