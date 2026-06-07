using System;
using KuendaFinance.Shared.Domain;

namespace KuendaFinance.Operations.Domain.Entities;

public class Transaction : Entity
{
    public Transaction(Guid id) : base(id) { }

    public Transaction() : base() { }

    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Category { get; set; } = string.Empty; // e.g. Salários, Energia, Renda, Empréstimo, Amortização, etc.
    public string Type { get; set; } = string.Empty; // Entrada, Saída
    public bool IsAuto { get; set; }
}
