using System;
using System.Threading;
using System.Threading.Tasks;

namespace KuendaFinance.IAM.Domain.Repositories;

public interface ITransactionManager
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
}
