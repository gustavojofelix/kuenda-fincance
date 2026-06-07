using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Application.Queries.GetClients;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.Operations.API.Endpoints.Clients;

public class GetClientsRequest
{
    [QueryParam]
    public int PageNumber { get; set; } = 1;
    [QueryParam]
    public int PageSize { get; set; } = 10;
    [QueryParam]
    public string SearchTerm { get; set; } = string.Empty;
    [QueryParam]
    public string Province { get; set; } = string.Empty;
    [QueryParam]
    public string Status { get; set; } = string.Empty;
    [QueryParam]
    public Guid? BranchId { get; set; }
}

public class GetClientsEndpoint : Endpoint<GetClientsRequest, PagedResult<ClientDto>>
{
    private readonly IMediator _mediator;

    public GetClientsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/clients");
        Claims("tenantId");
    }

    public override async Task HandleAsync(GetClientsRequest req, CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            AddError("Tenant Context missing or invalid.");
            ThrowIfAnyErrors();
            return;
        }

        // BranchId context
        Guid? finalBranchId = req.BranchId;
        if (finalBranchId == null || finalBranchId == Guid.Empty)
        {
            var branchHeader = HttpContext.Request.Headers["X-Branch-Id"].ToString();
            if (Guid.TryParse(branchHeader, out var parsedBranchId) && parsedBranchId != Guid.Empty)
            {
                finalBranchId = parsedBranchId;
            }
        }

        var query = new GetClientsQuery(
            req.PageNumber,
            req.PageSize,
            req.SearchTerm,
            req.Province,
            req.Status,
            finalBranchId
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
