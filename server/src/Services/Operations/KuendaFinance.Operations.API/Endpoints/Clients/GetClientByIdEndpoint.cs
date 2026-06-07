using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Application.Queries.GetClientById;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.Operations.API.Endpoints.Clients;

public class GetClientByIdRequest
{
    public Guid Id { get; set; }
}

public class GetClientByIdEndpoint : Endpoint<GetClientByIdRequest, ClientDto>
{
    private readonly IMediator _mediator;

    public GetClientByIdEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/clients/{id}");
        Claims("tenantId");
    }

    public override async Task HandleAsync(GetClientByIdRequest req, CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            AddError("Tenant Context missing or invalid.");
            ThrowIfAnyErrors();
            return;
        }

        var query = new GetClientByIdQuery(req.Id);
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Client.NotFound")
            {
                HttpContext.Response.StatusCode = 404;
                await HttpContext.Response.WriteAsJsonAsync(new { error = result.Error.Message }, ct);
                return;
            }

            AddError(result.Error.Message);
            ThrowIfAnyErrors();
            return;
        }

        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
    }
}
