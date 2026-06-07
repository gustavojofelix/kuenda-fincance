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

namespace KuendaFinance.Operations.Application.Commands.DisburseLoan;

public record DisburseLoanCommand(
    Guid LoanId,
    Guid TenantId,
    string DisbursementMethod,
    string DisbursementReference
) : ICommand<LoanDto>;

public class DisburseLoanCommandValidator : AbstractValidator<DisburseLoanCommand>
{
    public DisburseLoanCommandValidator()
    {
        RuleFor(x => x.LoanId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DisbursementMethod).NotEmpty().WithMessage("Disbursement method (e.g. M-Pesa, E-Mola, Banco) is required.");
        RuleFor(x => x.DisbursementReference).NotEmpty().WithMessage("Disbursement reference/receipt is required.");
    }
}

public class DisburseLoanCommandHandler : ICommandHandler<DisburseLoanCommand, LoanDto>
{
    private readonly ILoanRepository _loanRepository;

    public DisburseLoanCommandHandler(ILoanRepository loanRepository)
    {
        _loanRepository = loanRepository;
    }

    public async Task<Result<LoanDto>> Handle(DisburseLoanCommand request, CancellationToken cancellationToken)
    {
        var loan = await _loanRepository.GetByIdAsync(request.LoanId, cancellationToken);
        if (loan == null)
        {
            return Result.Failure<LoanDto>(new Error("Loan.NotFound", $"Loan with ID '{request.LoanId}' was not found."));
        }

        // Multi-tenancy check
        if (loan.TenantId != request.TenantId)
        {
            return Result.Failure<LoanDto>(new Error("Loan.Unauthorized", "You are not authorized to disburse this loan."));
        }

        if (loan.Status == "Active")
        {
            return Result.Failure<LoanDto>(new Error("Loan.AlreadyDisbursed", "This loan has already been disbursed and is active."));
        }

        if (loan.Status == "Paid")
        {
            return Result.Failure<LoanDto>(new Error("Loan.AlreadyPaid", "This loan has already been fully paid."));
        }

        // Update disbursement details
        var disburseDate = DateTime.UtcNow;
        loan.Status = "Active";
        loan.DisbursedAt = disburseDate;
        loan.DisbursementMethod = request.DisbursementMethod;
        loan.DisbursementReference = request.DisbursementReference;
        loan.NextPaymentDate = disburseDate.AddMonths(1);

        // Re-align installment due dates relative to the actual disbursement date
        int seq = 1;
        foreach (var inst in loan.Installments.OrderBy(x => x.InstallmentNumber))
        {
            inst.DueDate = disburseDate.AddMonths(seq);
            inst.Status = "Pending";
            seq++;
        }

        await _loanRepository.UpdateAsync(loan, cancellationToken);

        var dto = loan.ToDto();
        return Result.Success(dto);
    }
}
