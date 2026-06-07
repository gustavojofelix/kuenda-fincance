using System;

namespace KuendaFinance.Operations.Application.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Entrada, Saída
    public bool IsAuto { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AccountingMetricsDto
{
    public decimal CashBalance { get; set; }
    public decimal MonthlyInflow { get; set; }
    public decimal MonthlyOutflow { get; set; }
}
