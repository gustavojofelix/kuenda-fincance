using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Domain.Entities;
using KuendaFinance.Operations.Domain.Repositories;
using KuendaFinance.Shared.Messaging;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Commands.ReceivePayment;

public record ReceivePaymentCommand(
    Guid LoanId,
    Guid TenantId,
    decimal Amount,
    string Channel,
    string Reference
) : ICommand<LoanDto>;

public class ReceivePaymentCommandValidator : AbstractValidator<ReceivePaymentCommand>
{
    public ReceivePaymentCommandValidator()
    {
        RuleFor(x => x.LoanId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
        RuleFor(x => x.Channel).NotEmpty().WithMessage("Payment channel (e.g. M-Pesa, E-Mola, Banco) is required.");
        RuleFor(x => x.Reference).NotEmpty().WithMessage("Payment reference/receipt is required.");
    }
}

public class ReceivePaymentCommandHandler : ICommandHandler<ReceivePaymentCommand, LoanDto>
{
    private readonly ILoanRepository _loanRepository;
    private readonly ITransactionRepository _transactionRepository;

    public ReceivePaymentCommandHandler(ILoanRepository loanRepository, ITransactionRepository transactionRepository)
    {
        _loanRepository = loanRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<LoanDto>> Handle(ReceivePaymentCommand request, CancellationToken cancellationToken)
    {
        var loan = await _loanRepository.GetByIdAsync(request.LoanId, cancellationToken);
        if (loan == null)
        {
            return Result.Failure<LoanDto>(new Error("Loan.NotFound", $"Loan with ID '{request.LoanId}' was not found."));
        }

        // Multi-tenancy check
        if (loan.TenantId != request.TenantId)
        {
            return Result.Failure<LoanDto>(new Error("Loan.Unauthorized", "You are not authorized to register payments for this loan."));
        }

        if (loan.Status != "Active" && loan.Status != "Late")
        {
            return Result.Failure<LoanDto>(new Error("Loan.NotEligible", $"Cannot receive payment for loan in status '{loan.Status}'."));
        }

        // 1. Record the Payment
        var payment = new Payment(Guid.NewGuid())
        {
            LoanId = loan.Id,
            Amount = request.Amount,
            PaymentDate = DateTime.UtcNow,
            Channel = request.Channel,
            Reference = request.Reference
        };
        loan.Payments.Add(payment);

        // 2. Update Total Paid Amount
        loan.PaidAmount += request.Amount;

        // 3. Waterfall Payment Allocation Algorithm
        var remainingPayment = request.Amount;
        var installments = loan.Installments.OrderBy(i => i.InstallmentNumber).ToList();

        foreach (var inst in installments)
        {
            if (inst.PaidAmount < inst.TotalAmount)
            {
                var dueAmount = inst.TotalAmount - inst.PaidAmount;
                var allocated = Math.Min(remainingPayment, dueAmount);

                inst.PaidAmount += allocated;
                remainingPayment -= allocated;

                if (inst.PaidAmount >= inst.TotalAmount)
                {
                    inst.Status = "Paid";
                    inst.PaidAt = DateTime.UtcNow;
                }

                if (remainingPayment <= 0)
                {
                    break;
                }
            }
        }

        // 4. Update Loan Status and NextPaymentDate
        var firstUnpaid = installments.FirstOrDefault(i => i.Status != "Paid");
        if (firstUnpaid != null)
        {
            loan.NextPaymentDate = firstUnpaid.DueDate;
            
            // Check if any unpaid installments are past due, and mark late
            var isAnyLate = installments.Any(i => i.Status != "Paid" && i.DueDate < DateTime.UtcNow);
            loan.Status = isAnyLate ? "Late" : "Active";
        }
        else
        {
            loan.Status = "Paid";
            loan.NextPaymentDate = null;
        }

        await _loanRepository.UpdateAsync(loan, cancellationToken);

        // Auto-record payment received transaction
        var tx = new Transaction(Guid.NewGuid())
        {
            TenantId = loan.TenantId,
            BranchId = loan.BranchId,
            Description = $"Amortização ({request.Channel}): Contrato {loan.Id}",
            Amount = request.Amount,
            TransactionDate = DateTime.UtcNow,
            Category = "Amortização",
            Type = "Entrada",
            IsAuto = true
        };
        await _transactionRepository.AddAsync(tx, cancellationToken);

        var dto = loan.ToDto();
        return Result.Success(dto);
    }
}
