using System;
using KuendaFinance.Shared.Domain;

namespace KuendaFinance.Operations.Domain.Entities;

public class Installment : Entity
{
    public Installment(Guid id) : base(id) { }

    public Installment() : base() { }

    public Guid LoanId { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Principal { get; set; }
    public decimal Interest { get; set; }
    public decimal Penalty { get; set; } = 0;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Paid, Late
    public DateTime? PaidAt { get; set; }
}
