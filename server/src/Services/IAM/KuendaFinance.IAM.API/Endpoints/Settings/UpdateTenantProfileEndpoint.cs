using System;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.IAM.Application.DTOs;
using KuendaFinance.IAM.Application.Queries.TenantProfile;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.IAM.API.Endpoints.Settings;

public class UpdateTenantProfileEndpoint : Endpoint<UpdateTenantProfileRequest, TenantProfileDto>
{
    private readonly IMediator _mediator;

    public UpdateTenantProfileEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/api/tenants/profile");
        Claims("tenantId");
    }

    public override async Task HandleAsync(UpdateTenantProfileRequest req, CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            AddError("Tenant context missing or invalid.");
            ThrowIfAnyErrors();
            return;
        }

        var command = new UpdateTenantBrandingCommand(
            tenantId,
            req.Name,
            req.Email,
            req.Phone,
            req.Address,
            req.PrimaryColor,
            req.SecondaryColor,
            req.LogoUrl
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

public class UpdateTenantProfileRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
}
