using System;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Application.Interfaces;
using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Queries.Reporting;

public record GetPLReportQuery(Guid TenantId, Guid? BranchId, DateTime StartDate, DateTime EndDate) : IRequest<Result<ProfitAndLossReportDto>>;

public class GetPLReportQueryHandler : IRequestHandler<GetPLReportQuery, Result<ProfitAndLossReportDto>>
{
    private readonly IReportingService _reportingService;

    public GetPLReportQueryHandler(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    public async Task<Result<ProfitAndLossReportDto>> Handle(GetPLReportQuery request, CancellationToken cancellationToken)
    {
        var report = await _reportingService.GetPLReportAsync(request.TenantId, request.BranchId, request.StartDate, request.EndDate, cancellationToken);
        return Result.Success(report);
    }
}
