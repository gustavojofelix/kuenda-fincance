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

public class InviteTeamMemberEndpoint : Endpoint<InviteTeamMemberRequest, TeamUserDto>
{
    private readonly IMediator _mediator;

    public InviteTeamMemberEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/team");
        Claims("tenantId");
    }

    public override async Task HandleAsync(InviteTeamMemberRequest req, CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            AddError("Tenant context missing or invalid.");
            ThrowIfAnyErrors();
            return;
        }

        var branchRolesInput = new List<TeamBranchRoleInput>();
        if (req.BranchRoles != null)
        {
            foreach (var br in req.BranchRoles)
            {
                branchRolesInput.Add(new TeamBranchRoleInput(br.BranchId, br.Role));
            }
        }

        var command = new InviteTeamMemberCommand(
            tenantId,
            req.Name,
            req.Email,
            req.Phone,
            branchRolesInput
        );

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            AddError(result.Error.Message);
            ThrowIfAnyErrors();
            return;
        }

        HttpContext.Response.StatusCode = 201;
        await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
    }
}

public class InviteTeamMemberRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public List<InviteTeamMemberBranchRoleRequest>? BranchRoles { get; set; }
}

public class InviteTeamMemberBranchRoleRequest
{
    public Guid BranchId { get; set; }
    public string Role { get; set; } = string.Empty;
}
