using System;
using System.Collections.Generic;

namespace KuendaFinance.Operations.Application.DTOs;

public class LoanDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid ClientId { get; set; }
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? DisbursedAt { get; set; }
    public string? DisbursementMethod { get; set; }
    public string? DisbursementReference { get; set; }
    public decimal DailyPenaltyRate { get; set; }
    public decimal TotalToPay { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime? NextPaymentDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string? UpdatedBy { get; set; }

    public List<InstallmentDto> Installments { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
}

public class InstallmentDto
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Principal { get; set; }
    public decimal Interest { get; set; }
    public decimal Penalty { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
}
