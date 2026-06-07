using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.Operations.Application.Commands.DisburseLoan;
using KuendaFinance.Operations.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.Operations.API.Endpoints.Loans;

public class DisburseLoanRequest
{
    public Guid Id { get; set; } // Derived from route /api/loans/{id}/disburse
    public string DisbursementMethod { get; set; } = string.Empty;
    public string DisbursementReference { get; set; } = string.Empty;
}

public class DisburseLoanEndpoint : Endpoint<DisburseLoanRequest, LoanDto>
{
    private readonly IMediator _mediator;

    public DisburseLoanEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/loans/{id}/disburse");
        Claims("tenantId");
    }

    public override async Task HandleAsync(DisburseLoanRequest req, CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            AddError("Tenant Context missing or invalid.");
            ThrowIfAnyErrors();
            return;
        }

        var command = new DisburseLoanCommand(
            req.Id,
            tenantId,
            req.DisbursementMethod,
            req.DisbursementReference
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
