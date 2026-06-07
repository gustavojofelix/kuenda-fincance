using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Application.Interfaces;
using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Queries.Reporting;

public record GetMAP14Query(Guid TenantId, Guid? BranchId, DateTime StartDate, DateTime EndDate) : IRequest<Result<List<Map14ItemDto>>>;

public class GetMAP14QueryHandler : IRequestHandler<GetMAP14Query, Result<List<Map14ItemDto>>>
{
    private readonly IReportingService _reportingService;

    public GetMAP14QueryHandler(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    public async Task<Result<List<Map14ItemDto>>> Handle(GetMAP14Query request, CancellationToken cancellationToken)
    {
        var report = await _reportingService.GetMAP14ReportAsync(request.TenantId, request.BranchId, request.StartDate, request.EndDate, cancellationToken);
        return Result.Success(report);
    }
}
