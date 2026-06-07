using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Application.Interfaces;
using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Queries.Reporting;

public record GetCashFlowReportQuery(Guid TenantId, Guid? BranchId, DateTime StartDate, DateTime EndDate) : IRequest<Result<List<CashFlowReportItemDto>>>;

public class GetCashFlowReportQueryHandler : IRequestHandler<GetCashFlowReportQuery, Result<List<CashFlowReportItemDto>>>
{
    private readonly IReportingService _reportingService;

    public GetCashFlowReportQueryHandler(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    public async Task<Result<List<CashFlowReportItemDto>>> Handle(GetCashFlowReportQuery request, CancellationToken cancellationToken)
    {
        var report = await _reportingService.GetCashFlowReportAsync(request.TenantId, request.BranchId, request.StartDate, request.EndDate, cancellationToken);
        return Result.Success(report);
    }
}
