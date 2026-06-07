using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Application.Interfaces;
using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Queries.Reporting;

public record GetDisbursementsReportQuery(Guid TenantId, Guid? BranchId, DateTime StartDate, DateTime EndDate) : IRequest<Result<List<DisbursementReportItemDto>>>;

public class GetDisbursementsReportQueryHandler : IRequestHandler<GetDisbursementsReportQuery, Result<List<DisbursementReportItemDto>>>
{
    private readonly IReportingService _reportingService;

    public GetDisbursementsReportQueryHandler(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    public async Task<Result<List<DisbursementReportItemDto>>> Handle(GetDisbursementsReportQuery request, CancellationToken cancellationToken)
    {
        var report = await _reportingService.GetDisbursementsReportAsync(request.TenantId, request.BranchId, request.StartDate, request.EndDate, cancellationToken);
        return Result.Success(report);
    }
}
