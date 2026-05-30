using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Shared.Messaging;

public interface ICommand : IRequest<Result>, ICommandBase
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, ICommandBase
{
}
