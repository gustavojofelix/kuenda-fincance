using System;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Application.Queries.GetAccountingMetrics;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.Operations.API.Endpoints.Transactions;

public class GetAccountingMetricsRequest
{
    public Guid? BranchId { get; set; }
}

public class GetAccountingMetricsEndpoint : Endpoint<GetAccountingMetricsRequest, AccountingMetricsDto>
{
    private readonly IMediator _mediator;

    public GetAccountingMetricsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/transactions/metrics");
        Claims("tenantId");
    }

    public override async Task HandleAsync(GetAccountingMetricsRequest req, CancellationToken ct)
    {
        Guid? branchId = req.BranchId;
        if (branchId == null || branchId == Guid.Empty)
        {
            var branchHeader = HttpContext.Request.Headers["X-Branch-Id"].ToString();
            if (Guid.TryParse(branchHeader, out var parsedBranchId))
            {
                branchId = parsedBranchId;
            }
        }

        var query = new GetAccountingMetricsQuery(branchId);
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            AddError(result.Error.Message);
            ThrowIfAnyErrors();
            return;
        }

        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
    }
}
