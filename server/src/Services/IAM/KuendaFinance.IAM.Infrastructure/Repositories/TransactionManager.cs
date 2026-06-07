using System;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.IAM.Domain.Repositories;
using KuendaFinance.IAM.Infrastructure.Persistence;

namespace KuendaFinance.IAM.Infrastructure.Repositories;

public class TransactionManager : ITransactionManager
{
    private readonly IamDbContext _context;

    public TransactionManager(IamDbContext context)
    {
        _context = context;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action();
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
