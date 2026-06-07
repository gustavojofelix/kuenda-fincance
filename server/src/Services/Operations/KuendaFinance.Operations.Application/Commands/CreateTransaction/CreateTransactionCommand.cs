using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Domain.Entities;
using KuendaFinance.Operations.Domain.Repositories;
using KuendaFinance.Shared.Messaging;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Commands.CreateTransaction;

public record CreateTransactionCommand(
    Guid TenantId,
    Guid BranchId,
    string Description,
    decimal Amount,
    string Category,
    string Type,
    DateTime TransactionDate
) : ICommand<TransactionDto>;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
        RuleFor(x => x.Category).NotEmpty().WithMessage("Category is required.");
        RuleFor(x => x.Type).Must(t => t == "Entrada" || t == "Saída").WithMessage("Type must be either 'Entrada' or 'Saída'.");
    }
}

public class CreateTransactionCommandHandler : ICommandHandler<CreateTransactionCommand, TransactionDto>
{
    private readonly ITransactionRepository _transactionRepository;

    public CreateTransactionCommandHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<TransactionDto>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var tx = new Transaction(Guid.NewGuid())
        {
            TenantId = request.TenantId,
            BranchId = request.BranchId,
            Description = request.Description,
            Amount = request.Amount,
            TransactionDate = request.TransactionDate == default ? DateTime.UtcNow : request.TransactionDate.ToUniversalTime(),
            Category = request.Category,
            Type = request.Type,
            IsAuto = false
        };

        await _transactionRepository.AddAsync(tx, cancellationToken);

        return Result.Success(tx.ToDto());
    }
}
