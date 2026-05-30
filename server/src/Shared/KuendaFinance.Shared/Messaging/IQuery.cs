using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Shared.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
