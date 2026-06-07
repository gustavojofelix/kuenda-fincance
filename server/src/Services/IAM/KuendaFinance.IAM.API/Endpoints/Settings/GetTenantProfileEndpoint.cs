using System;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.IAM.Application.DTOs;
using KuendaFinance.IAM.Application.Queries.TenantProfile;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.IAM.API.Endpoints.Settings;

public class GetTenantProfileEndpoint : EndpointWithoutRequest<TenantProfileDto>
{
    private readonly IMediator _mediator;

    public GetTenantProfileEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/tenants/profile");
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

        var query = new GetTenantProfileQuery(tenantId);
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
