using System.Linq;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Domain.Entities;

namespace KuendaFinance.Operations.Application.DTOs;

public static class LoanMappingExtensions
{
    public static LoanDto ToDto(this Loan loan)
    {
        return new LoanDto
        {
            Id = loan.Id,
            TenantId = loan.TenantId,
            BranchId = loan.BranchId,
            ClientId = loan.ClientId,
            Amount = loan.Amount,
            InterestRate = loan.InterestRate,
            TermMonths = loan.TermMonths,
            Status = loan.Status,
            DisbursedAt = loan.DisbursedAt,
            DisbursementMethod = loan.DisbursementMethod,
            DisbursementReference = loan.DisbursementReference,
            DailyPenaltyRate = loan.DailyPenaltyRate,
            TotalToPay = loan.TotalToPay,
            PaidAmount = loan.PaidAmount,
            NextPaymentDate = loan.NextPaymentDate,
            CreatedAt = loan.CreatedAt,
            CreatedBy = loan.CreatedBy,
            LastUpdated = loan.LastUpdated,
            UpdatedBy = loan.UpdatedBy,
            Installments = loan.Installments.Select(inst => inst.ToDto()).OrderBy(x => x.InstallmentNumber).ToList(),
            Payments = loan.Payments.Select(p => p.ToDto()).OrderBy(x => x.PaymentDate).ToList()
        };
    }

    public static InstallmentDto ToDto(this Installment inst)
    {
        return new InstallmentDto
        {
            Id = inst.Id,
            LoanId = inst.LoanId,
            InstallmentNumber = inst.InstallmentNumber,
            DueDate = inst.DueDate,
            Principal = inst.Principal,
            Interest = inst.Interest,
            Penalty = inst.Penalty,
            TotalAmount = inst.TotalAmount,
            PaidAmount = inst.PaidAmount,
            Status = inst.Status,
            PaidAt = inst.PaidAt
        };
    }

    public static PaymentDto ToDto(this Payment p)
    {
        return new PaymentDto
        {
            Id = p.Id,
            LoanId = p.LoanId,
            Amount = p.Amount,
            PaymentDate = p.PaymentDate,
            Channel = p.Channel,
            Reference = p.Reference
        };
    }
}
