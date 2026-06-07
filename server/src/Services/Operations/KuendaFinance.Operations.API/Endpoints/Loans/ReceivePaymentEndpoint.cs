using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.Operations.Application.Commands.ReceivePayment;
using KuendaFinance.Operations.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.Operations.API.Endpoints.Loans;

public class ReceivePaymentRequest
{
    public Guid Id { get; set; } // Derived from route /api/loans/{id}/payments
    public decimal Amount { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
}

public class ReceivePaymentEndpoint : Endpoint<ReceivePaymentRequest, LoanDto>
{
    private readonly IMediator _mediator;

    public ReceivePaymentEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/loans/{id}/payments");
        Claims("tenantId");
    }

    public override async Task HandleAsync(ReceivePaymentRequest req, CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            AddError("Tenant Context missing or invalid.");
            ThrowIfAnyErrors();
            return;
        }

        var command = new ReceivePaymentCommand(
            req.Id,
            tenantId,
            req.Amount,
            req.Channel,
            req.Reference
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
