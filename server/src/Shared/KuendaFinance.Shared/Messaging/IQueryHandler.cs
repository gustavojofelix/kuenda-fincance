using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Shared.Messaging;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
