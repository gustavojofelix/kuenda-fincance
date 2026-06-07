using System;

namespace KuendaFinance.Operations.Application.DTOs;

public class DashboardMetricsDto
{
    public int TotalClients { get; set; }
    public int ActiveClients { get; set; }
    public int AtRiskClients { get; set; }
    public decimal TotalActivePortfolio { get; set; }
    public int DueTodayCount { get; set; }
    public decimal Par30Rate { get; set; }
    public decimal CashBalance { get; set; }
    public decimal MonthlyInflow { get; set; }
    public decimal MonthlyOutflow { get; set; }
}

public class Map10ItemDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountDescription { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class Map14ItemDto
{
    public string ContractId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public decimal ExposureRatio { get; set; } // Percentage of equity
}

public class ActivePortfolioItemDto
{
    public string ContractId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal UnpaidBalance { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class Par30ItemDto
{
    public string ContractId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal UnpaidBalance { get; set; }
    public DateTime? NextPaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DisbursementReportItemDto
{
    public string ContractId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateTime DisbursementDate { get; set; }
    public decimal PrincipalAmount { get; set; }
    public int TermMonths { get; set; }
    public string DisbursementMethod { get; set; } = string.Empty;
}

public class CashFlowReportItemDto
{
    public string TransactionId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Entrada, Saída
    public decimal Amount { get; set; }
}

public class ProfitAndLossReportDto
{
    public decimal InterestInflow { get; set; }
    public decimal SalariesOutflow { get; set; }
    public decimal RentOutflow { get; set; }
    public decimal UtilitiesOutflow { get; set; }
    public decimal NetProfit { get; set; }
}
