using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Application.Interfaces;
using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Queries.Reporting;

public record GetPAR30Query(Guid TenantId, Guid? BranchId) : IRequest<Result<List<Par30ItemDto>>>;

public class GetPAR30QueryHandler : IRequestHandler<GetPAR30Query, Result<List<Par30ItemDto>>>
{
    private readonly IReportingService _reportingService;

    public GetPAR30QueryHandler(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    public async Task<Result<List<Par30ItemDto>>> Handle(GetPAR30Query request, CancellationToken cancellationToken)
    {
        var report = await _reportingService.GetPAR30ReportAsync(request.TenantId, request.BranchId, cancellationToken);
        return Result.Success(report);
    }
}
