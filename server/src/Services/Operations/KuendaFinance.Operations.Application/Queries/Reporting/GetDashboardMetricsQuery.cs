using System;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Application.Interfaces;
using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Queries.Reporting;

public record GetDashboardMetricsQuery(Guid TenantId, Guid? BranchId) : IRequest<Result<DashboardMetricsDto>>;

public class GetDashboardMetricsQueryHandler : IRequestHandler<GetDashboardMetricsQuery, Result<DashboardMetricsDto>>
{
    private readonly IReportingService _reportingService;

    public GetDashboardMetricsQueryHandler(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    public async Task<Result<DashboardMetricsDto>> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        var metrics = await _reportingService.GetDashboardMetricsAsync(request.TenantId, request.BranchId, cancellationToken);
        return Result.Success(metrics);
    }
}
