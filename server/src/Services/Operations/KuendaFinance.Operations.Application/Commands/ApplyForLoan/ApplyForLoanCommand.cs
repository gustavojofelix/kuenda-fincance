using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Domain.Entities;
using KuendaFinance.Operations.Domain.Repositories;
using KuendaFinance.Shared.Messaging;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Commands.ApplyForLoan;

public record ApplyForLoanCommand(
    Guid TenantId,
    Guid BranchId,
    Guid ClientId,
    decimal Amount,
    decimal InterestRate,
    int TermMonths,
    decimal DailyPenaltyRate
) : ICommand<LoanDto>;

public class ApplyForLoanCommandValidator : AbstractValidator<ApplyForLoanCommand>
{
    public ApplyForLoanCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Loan amount must be greater than zero.");
        RuleFor(x => x.InterestRate).GreaterThanOrEqualTo(0).WithMessage("Interest rate cannot be negative.");
        RuleFor(x => x.TermMonths).GreaterThan(0).WithMessage("Term must be at least 1 month.");
        RuleFor(x => x.DailyPenaltyRate).GreaterThanOrEqualTo(0).WithMessage("Daily penalty rate cannot be negative.");
    }
}

public class ApplyForLoanCommandHandler : ICommandHandler<ApplyForLoanCommand, LoanDto>
{
    private readonly ILoanRepository _loanRepository;
    private readonly IClientRepository _clientRepository;

    public ApplyForLoanCommandHandler(ILoanRepository loanRepository, IClientRepository clientRepository)
    {
        _loanRepository = loanRepository;
        _clientRepository = clientRepository;
    }

    public async Task<Result<LoanDto>> Handle(ApplyForLoanCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify Client Exists
        var client = await _clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
        if (client == null)
        {
            return Result.Failure<LoanDto>(new Error("Client.NotFound", $"Client with ID '{request.ClientId}' was not found."));
        }

        // Check Tenant boundaries
        if (client.TenantId != request.TenantId)
        {
            return Result.Failure<LoanDto>(new Error("Client.Unauthorized", "Client context does not match user tenant."));
        }

        // 2. Setup Amortization Variables
        var loanId = Guid.NewGuid();
        var amount = (double)request.Amount;
        var monthlyRate = (double)request.InterestRate / 100.0;
        var term = request.TermMonths;

        double monthlyPayment = 0;
        if (monthlyRate == 0)
        {
            monthlyPayment = amount / term;
        }
        else
        {
            monthlyPayment = (amount * monthlyRate) / (1.0 - Math.Pow(1.0 + monthlyRate, -term));
        }

        monthlyPayment = Math.Round(monthlyPayment, 2);
        var totalToPay = (decimal)Math.Round(monthlyPayment * term, 2);

        var loan = new Loan(loanId)
        {
            TenantId = request.TenantId,
            BranchId = request.BranchId,
            ClientId = request.ClientId,
            Amount = request.Amount,
            InterestRate = request.InterestRate,
            TermMonths = request.TermMonths,
            DailyPenaltyRate = request.DailyPenaltyRate,
            Status = "Evaluation",
            TotalToPay = totalToPay,
            PaidAmount = 0
        };

        // 3. Generate Installments list
        var remainingBalance = amount;
        var startDate = DateTime.UtcNow;

        for (int i = 1; i <= term; i++)
        {
            double interestPortion = Math.Round(remainingBalance * monthlyRate, 2);
            double principalPortion = Math.Round(monthlyPayment - interestPortion, 2);

            // Handle last installment rounding difference
            if (i == term)
            {
                principalPortion = Math.Round(remainingBalance, 2);
                monthlyPayment = principalPortion + interestPortion;
            }

            remainingBalance -= principalPortion;

            var dueDate = startDate.AddMonths(i);
            loan.Installments.Add(new Installment(Guid.NewGuid())
            {
                LoanId = loanId,
                InstallmentNumber = i,
                DueDate = dueDate,
                Principal = (decimal)principalPortion,
                Interest = (decimal)interestPortion,
                TotalAmount = (decimal)monthlyPayment,
                PaidAmount = 0,
                Status = "Pending"
            });
        }

        await _loanRepository.AddAsync(loan, cancellationToken);

        var dto = loan.ToDto();
        return Result.Success(dto);
    }
}
