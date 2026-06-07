using System;
using KuendaFinance.Shared.Domain;

namespace KuendaFinance.Operations.Domain.Entities;

public class CreditSettings : Entity
{
    public CreditSettings(Guid id) : base(id) { }

    public CreditSettings() : base() { }

    public Guid TenantId { get; set; }
    public decimal DefaultInterestRate { get; set; } = 5.0m;
    public decimal DefaultPenaltyRate { get; set; } = 1.0m;
    public decimal OriginationFee { get; set; } = 500.0m;
    public int MaxTermMonths { get; set; } = 24;
    public string Currency { get; set; } = "MZN";
}
