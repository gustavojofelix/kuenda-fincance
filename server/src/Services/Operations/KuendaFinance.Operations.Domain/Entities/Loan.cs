using System;
using System.Collections.Generic;
using KuendaFinance.Shared.Domain;

namespace KuendaFinance.Operations.Domain.Entities;

public class Loan : Entity
{
    public Loan(Guid id) : base(id)
    {
        Installments = new List<Installment>();
        Payments = new List<Payment>();
    }

    public Loan() : base()
    {
        Installments = new List<Installment>();
        Payments = new List<Payment>();
    }

    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid ClientId { get; set; }
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public string Status { get; set; } = "Evaluation";
    public DateTime? DisbursedAt { get; set; }
    public string? DisbursementMethod { get; set; }
    public string? DisbursementReference { get; set; }
    public decimal DailyPenaltyRate { get; set; } = 0.01m;
    public decimal TotalToPay { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime? NextPaymentDate { get; set; }

    public ICollection<Installment> Installments { get; set; }
    public ICollection<Payment> Payments { get; set; }
}
