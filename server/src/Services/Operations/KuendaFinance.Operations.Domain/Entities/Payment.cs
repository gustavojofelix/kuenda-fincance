using System;
using KuendaFinance.Shared.Domain;

namespace KuendaFinance.Operations.Domain.Entities;

public class Payment : Entity
{
    public Payment(Guid id) : base(id) { }

    public Payment() : base() { }

    public Guid LoanId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string Channel { get; set; } = string.Empty; // M-Pesa, E-Mola, Banco
    public string Reference { get; set; } = string.Empty;
}
