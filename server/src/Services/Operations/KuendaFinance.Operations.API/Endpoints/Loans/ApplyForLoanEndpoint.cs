using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.Operations.Application.Commands.ApplyForLoan;
using KuendaFinance.Operations.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.Operations.API.Endpoints.Loans;

public class ApplyForLoanRequest
{
    public Guid? BranchId { get; set; }
    public Guid ClientId { get; set; }
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public decimal DailyPenaltyRate { get; set; } = 0.01m;
}

public class ApplyForLoanEndpoint : Endpoint<ApplyForLoanRequest, LoanDto>
{
    private readonly IMediator _mediator;

    public ApplyForLoanEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/loans");
        Claims("tenantId");
    }

    public override async Task HandleAsync(ApplyForLoanRequest req, CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            AddError("Tenant Context missing or invalid.");
            ThrowIfAnyErrors();
            return;
        }

        Guid? branchId = req.BranchId;
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

        var command = new ApplyForLoanCommand(
            tenantId,
            branchId.Value,
            req.ClientId,
            req.Amount,
            req.InterestRate,
            req.TermMonths,
            req.DailyPenaltyRate
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
