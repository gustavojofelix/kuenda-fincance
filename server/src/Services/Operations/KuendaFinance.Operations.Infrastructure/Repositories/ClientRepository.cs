using System;
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
}
