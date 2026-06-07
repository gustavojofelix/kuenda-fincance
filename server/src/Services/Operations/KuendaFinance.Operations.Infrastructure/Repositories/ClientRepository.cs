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

public class ClientRepository : IClientRepository
{
    private readonly OperationsDbContext _context;

    public ClientRepository(OperationsDbContext context)
    {
        _context = context;
    }

    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Clients
            .Include(c => c.Guarantees)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Client?> GetByBiAsync(string bi, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Clients
            .Include(c => c.Guarantees)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.BI == bi, cancellationToken);
    }

    public async Task AddAsync(Client client, CancellationToken cancellationToken = default)
    {
        await _context.Clients.AddAsync(client, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Client client, CancellationToken cancellationToken = default)
    {
        _context.Clients.Update(client);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(List<Client> Items, int TotalCount)> GetClientsPagedAsync(
        int pageNumber,
        int pageSize,
        string searchTerm,
        string province,
        string status,
        Guid? branchId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Clients.Include(c => c.Guarantees).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search) || 
                                     c.BI.ToLower().Contains(search) || 
                                     c.Phone.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(province) && !string.Equals(province, "Todas", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(c => c.Province == province);
        }

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "Todos", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(c => c.Status == status);
        }

        if (branchId.HasValue && branchId != Guid.Empty)
        {
            query = query.Where(c => c.BranchId == branchId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddStatusHistoryAsync(ClientStatusHistory history, CancellationToken cancellationToken = default)
    {
        await _context.ClientStatusHistories.AddAsync(history, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
