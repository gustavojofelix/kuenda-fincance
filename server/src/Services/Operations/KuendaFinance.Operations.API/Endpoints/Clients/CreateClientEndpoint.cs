using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.Operations.Application.Commands.CreateClient;
using KuendaFinance.Operations.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.Operations.API.Endpoints.Clients;

public class CreateClientRequest
{
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
    public List<CreateGuaranteeInput> Guarantees { get; set; } = new();
}

public class CreateClientEndpoint : Endpoint<CreateClientRequest, ClientDto>
{
    private readonly IMediator _mediator;

    public CreateClientEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/clients");
        Claims("tenantId"); // Must have tenantId claim
    }

    public override async Task HandleAsync(CreateClientRequest req, CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            AddError("Tenant Context missing or invalid.");
            ThrowIfAnyErrors();
            return;
        }

        // Try extracting BranchId from Header if not specified in request body
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

        var command = new CreateClientCommand(
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
            req.Guarantees
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
