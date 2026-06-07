using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Application.Interfaces;
using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Queries.Reporting;

public record GetMAP10Query(Guid TenantId, Guid? BranchId, DateTime StartDate, DateTime EndDate) : IRequest<Result<List<Map10ItemDto>>>;

public class GetMAP10QueryHandler : IRequestHandler<GetMAP10Query, Result<List<Map10ItemDto>>>
{
    private readonly IReportingService _reportingService;

    public GetMAP10QueryHandler(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    public async Task<Result<List<Map10ItemDto>>> Handle(GetMAP10Query request, CancellationToken cancellationToken)
    {
        var report = await _reportingService.GetMAP10ReportAsync(request.TenantId, request.BranchId, request.StartDate, request.EndDate, cancellationToken);
        return Result.Success(report);
    }
}
