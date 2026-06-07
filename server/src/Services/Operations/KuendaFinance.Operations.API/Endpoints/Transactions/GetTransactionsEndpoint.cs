using System;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Application.Queries.GetTransactions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.Operations.API.Endpoints.Transactions;

public class GetTransactionsRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SearchTerm { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
}

public class GetTransactionsEndpoint : Endpoint<GetTransactionsRequest, PagedResult<TransactionDto>>
{
    private readonly IMediator _mediator;

    public GetTransactionsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/transactions");
        Claims("tenantId");
    }

    public override async Task HandleAsync(GetTransactionsRequest req, CancellationToken ct)
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

        var query = new GetTransactionsQuery(
            req.PageNumber,
            req.PageSize,
            req.SearchTerm,
            req.Category,
            req.Type,
            branchId
        );

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
