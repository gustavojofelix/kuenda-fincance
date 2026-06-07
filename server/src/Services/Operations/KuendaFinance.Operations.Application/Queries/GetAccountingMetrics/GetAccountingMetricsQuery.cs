using System;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Domain.Repositories;
using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Queries.GetAccountingMetrics;

public record GetAccountingMetricsQuery(Guid? BranchId = null) : IRequest<Result<AccountingMetricsDto>>;

public class GetAccountingMetricsQueryHandler : IRequestHandler<GetAccountingMetricsQuery, Result<AccountingMetricsDto>>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetAccountingMetricsQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<AccountingMetricsDto>> Handle(GetAccountingMetricsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var cashBalance = await _transactionRepository.GetCurrentBalanceAsync(request.BranchId, cancellationToken);
        var (inflow, outflow) = await _transactionRepository.GetMonthlyMetricsAsync(request.BranchId, now.Year, now.Month, cancellationToken);

        var dto = new AccountingMetricsDto
        {
            CashBalance = cashBalance,
            MonthlyInflow = inflow,
            MonthlyOutflow = outflow
        };

        return Result.Success(dto);
    }
}
