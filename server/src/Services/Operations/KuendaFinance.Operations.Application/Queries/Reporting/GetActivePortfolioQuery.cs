using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Application.Interfaces;
using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Queries.Reporting;

public record GetActivePortfolioQuery(Guid TenantId, Guid? BranchId) : IRequest<Result<List<ActivePortfolioItemDto>>>;

public class GetActivePortfolioQueryHandler : IRequestHandler<GetActivePortfolioQuery, Result<List<ActivePortfolioItemDto>>>
{
    private readonly IReportingService _reportingService;

    public GetActivePortfolioQueryHandler(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    public async Task<Result<List<ActivePortfolioItemDto>>> Handle(GetActivePortfolioQuery request, CancellationToken cancellationToken)
    {
        var report = await _reportingService.GetActivePortfolioReportAsync(request.TenantId, request.BranchId, cancellationToken);
        return Result.Success(report);
    }
}
