using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Domain.Repositories;
using KuendaFinance.Shared.Messaging;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Commands.ProcessDailyPenalties;

public record ProcessDailyPenaltiesCommand : ICommand<ProcessDailyPenaltiesResult>;

public record ProcessDailyPenaltiesResult(int LoansProcessed, int InstallmentsPenalized, decimal TotalPenaltiesAccrued);

public class ProcessDailyPenaltiesCommandHandler : ICommandHandler<ProcessDailyPenaltiesCommand, ProcessDailyPenaltiesResult>
{
    private readonly ILoanRepository _loanRepository;

    public ProcessDailyPenaltiesCommandHandler(ILoanRepository loanRepository)
    {
        _loanRepository = loanRepository;
    }

    public async Task<Result<ProcessDailyPenaltiesResult>> Handle(ProcessDailyPenaltiesCommand request, CancellationToken cancellationToken)
    {
        var loans = await _loanRepository.GetActiveAndLateLoansAsync(cancellationToken);
        
        int loansProcessed = 0;
        int installmentsPenalized = 0;
        decimal totalPenaltiesAccrued = 0;

        foreach (var loan in loans)
        {
            bool isLoanUpdated = false;
            var installments = loan.Installments.OrderBy(i => i.InstallmentNumber).ToList();

            foreach (var inst in installments)
            {
                // If the installment is past its due date and is not fully paid
                if (inst.DueDate < DateTime.UtcNow && inst.Status != "Paid")
                {
                    // Mark installment as Late
                    if (inst.Status != "Late")
                    {
                        inst.Status = "Late";
                        isLoanUpdated = true;
                    }

                    // Mark loan as Late
                    if (loan.Status != "Late")
                    {
                        loan.Status = "Late";
                        isLoanUpdated = true;
                    }

                    // Calculate daily penalty based on the loan's specific penalty rate
                    decimal unpaidBalance = inst.TotalAmount - inst.PaidAmount;
                    if (unpaidBalance > 0)
                    {
                        decimal penalty = Math.Round(unpaidBalance * loan.DailyPenaltyRate, 2);
                        if (penalty > 0)
                        {
                            inst.Penalty += penalty;
                            inst.TotalAmount += penalty;
                            loan.TotalToPay += penalty;
                            
                            installmentsPenalized++;
                            totalPenaltiesAccrued += penalty;
                            isLoanUpdated = true;
                        }
                    }
                }
            }

            if (isLoanUpdated)
            {
                // Update next payment date to the oldest unpaid installment
                var firstUnpaid = installments.FirstOrDefault(i => i.Status != "Paid");
                if (firstUnpaid != null)
                {
                    loan.NextPaymentDate = firstUnpaid.DueDate;
                }

                await _loanRepository.UpdateAsync(loan, cancellationToken);
                loansProcessed++;
            }
        }

        return Result.Success(new ProcessDailyPenaltiesResult(loansProcessed, installmentsPenalized, totalPenaltiesAccrued));
    }
}
