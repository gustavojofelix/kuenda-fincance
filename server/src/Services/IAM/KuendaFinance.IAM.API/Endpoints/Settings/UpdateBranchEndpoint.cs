using System;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.IAM.Application.DTOs;
using KuendaFinance.IAM.Application.Queries.Branches;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.IAM.API.Endpoints.Settings;

public class UpdateBranchEndpoint : Endpoint<UpdateBranchRequest, BranchDto>
{
    private readonly IMediator _mediator;

    public UpdateBranchEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/api/branches/{id}");
        Claims("tenantId");
    }

    public override async Task HandleAsync(UpdateBranchRequest req, CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            AddError("Tenant context missing or invalid.");
            ThrowIfAnyErrors();
            return;
        }

        var idRoute = Route<Guid>("id");

        var command = new UpdateBranchCommand(
            idRoute,
            tenantId,
            req.Name,
            req.Cellphone,
            req.Email,
            req.City,
            req.Address,
            req.Manager,
            req.Status
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

public class UpdateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public string Cellphone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Manager { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
