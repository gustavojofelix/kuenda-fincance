using System;

namespace KuendaFinance.Operations.Application.DTOs;

public class CreditSettingsDto
{
    public Guid TenantId { get; set; }
    public decimal DefaultInterestRate { get; set; }
    public decimal DefaultPenaltyRate { get; set; }
    public decimal OriginationFee { get; set; }
    public int MaxTermMonths { get; set; }
    public string Currency { get; set; } = "MZN";
}
