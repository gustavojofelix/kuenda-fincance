using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.IAM.Application.DTOs;
using KuendaFinance.IAM.Application.Queries.Team;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.IAM.API.Endpoints.Settings;

public class GetTeamEndpoint : EndpointWithoutRequest<List<TeamUserDto>>
{
    private readonly IMediator _mediator;

    public GetTeamEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/team");
        Claims("tenantId");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            AddError("Tenant context missing or invalid.");
            ThrowIfAnyErrors();
            return;
        }

        var query = new GetTeamMembersQuery(tenantId);
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
