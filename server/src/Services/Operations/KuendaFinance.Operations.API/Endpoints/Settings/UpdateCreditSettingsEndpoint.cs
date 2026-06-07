using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.Operations.Application.Commands.CreditSettings;
using KuendaFinance.Operations.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.Operations.API.Endpoints.Settings;

public class UpdateCreditSettingsRequest
{
    public decimal DefaultInterestRate { get; set; }
    public decimal DefaultPenaltyRate { get; set; }
    public decimal OriginationFee { get; set; }
    public int MaxTermMonths { get; set; }
    public string Currency { get; set; } = "MZN";
}

public class UpdateCreditSettingsEndpoint : Endpoint<UpdateCreditSettingsRequest, CreditSettingsDto>
{
    private readonly IMediator _mediator;

    public UpdateCreditSettingsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/api/credit-settings");
        Claims("tenantId");
    }

    public override async Task HandleAsync(UpdateCreditSettingsRequest req, CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            AddError("Tenant Context missing or invalid.");
            ThrowIfAnyErrors();
            return;
        }

        var command = new UpdateCreditSettingsCommand(
            tenantId,
            req.DefaultInterestRate,
            req.DefaultPenaltyRate,
            req.OriginationFee,
            req.MaxTermMonths,
            req.Currency
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
