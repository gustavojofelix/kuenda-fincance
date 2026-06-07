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

public class UpdateTeamMemberEndpoint : Endpoint<UpdateTeamMemberRequest, TeamUserDto>
{
    private readonly IMediator _mediator;

    public UpdateTeamMemberEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/api/team/{id}");
        Claims("tenantId");
    }

    public override async Task HandleAsync(UpdateTeamMemberRequest req, CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            AddError("Tenant context missing or invalid.");
            ThrowIfAnyErrors();
            return;
        }

        var idRoute = Route<Guid>("id");

        var branchRolesInput = new List<TeamBranchRoleInput>();
        if (req.BranchRoles != null)
        {
            foreach (var br in req.BranchRoles)
            {
                branchRolesInput.Add(new TeamBranchRoleInput(br.BranchId, br.Role));
            }
        }

        var command = new UpdateTeamMemberCommand(
            idRoute,
            tenantId,
            req.Name,
            req.Phone,
            req.Status,
            branchRolesInput
        );

        var result = await _mediator.Send(command, ct);

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

public class UpdateTeamMemberRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<UpdateTeamMemberBranchRoleRequest>? BranchRoles { get; set; }
}

public class UpdateTeamMemberBranchRoleRequest
{
    public Guid BranchId { get; set; }
    public string Role { get; set; } = string.Empty;
}
