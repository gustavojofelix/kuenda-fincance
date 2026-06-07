using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using KuendaFinance.Operations.Application.Commands.CreateTransaction;
using KuendaFinance.Operations.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace KuendaFinance.Operations.API.Endpoints.Transactions;

public class CreateTransactionRequest
{
    public Guid? BranchId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Entrada, Saída
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
}

public class CreateTransactionEndpoint : Endpoint<CreateTransactionRequest, TransactionDto>
{
    private readonly IMediator _mediator;

    public CreateTransactionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/transactions");
        Claims("tenantId");
    }

    public override async Task HandleAsync(CreateTransactionRequest req, CancellationToken ct)
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

        var command = new CreateTransactionCommand(
            tenantId,
            branchId.Value,
            req.Description,
            req.Amount,
            req.Category,
            req.Type,
            req.TransactionDate
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
