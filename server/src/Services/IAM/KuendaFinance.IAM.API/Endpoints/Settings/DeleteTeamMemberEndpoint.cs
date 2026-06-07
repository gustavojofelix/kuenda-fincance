using System;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.IAM.Application.Queries.Team;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.IAM.API.Endpoints.Settings;

public class DeleteTeamMemberEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public DeleteTeamMemberEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("/api/team/{id}");
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

        var idRoute = Route<Guid>("id");
        var command = new DeleteTeamMemberCommand(idRoute, tenantId);
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            AddError(result.Error.Message);
            ThrowIfAnyErrors();
            return;
        }

        HttpContext.Response.StatusCode = 204;
        return;
    }
}
