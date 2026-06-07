using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.Operations.Application.Commands.UpdateClient;
using KuendaFinance.Operations.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.Operations.API.Endpoints.Clients;

public class UpdateClientRequest
{
    public Guid Id { get; set; } // Derived from route /api/clients/{id}
    public Guid? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BI { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string MaritalStatus { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Business { get; set; } = string.Empty;
    public string BusinessYears { get; set; } = string.Empty;
    public string Income { get; set; } = string.Empty;
    public string EmergencyName { get; set; } = string.Empty;
    public string EmergencyRelation { get; set; } = string.Empty;
    public string EmergencyPhone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<UpdateGuaranteeInput> Guarantees { get; set; } = new();
}

public class UpdateClientEndpoint : Endpoint<UpdateClientRequest, ClientDto>
{
    private readonly IMediator _mediator;

    public UpdateClientEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/api/clients/{id}");
        Claims("tenantId"); // Must have tenantId claim
    }

    public override async Task HandleAsync(UpdateClientRequest req, CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            AddError("Tenant Context missing or invalid.");
            ThrowIfAnyErrors();
            return;
        }

        var branchId = req.BranchId;
        if (branchId == null || branchId == Guid.Empty)
        {
            var branchHeader = HttpContext.Request.Headers["X-Branch-Id"].ToString();
            if (Guid.TryParse(branchHeader, out var parsedBranchId))
            {
                branchId = parsedBranchId;
            }
            else
            {
                AddError("BranchId or X-Branch-Id header is required.");
                ThrowIfAnyErrors();
                return;
            }
        }

        var command = new UpdateClientCommand(
            req.Id,
            tenantId,
            branchId.Value,
            req.Name,
            req.BI,
            req.Phone,
            req.MaritalStatus,
            req.Province,
            req.District,
            req.Neighborhood,
            req.Address,
            req.Business,
            req.BusinessYears,
            req.Income,
            req.EmergencyName,
            req.EmergencyRelation,
            req.EmergencyPhone,
            req.Status,
            req.Guarantees
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
