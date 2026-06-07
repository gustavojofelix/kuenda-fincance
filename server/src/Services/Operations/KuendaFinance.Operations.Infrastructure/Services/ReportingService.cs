using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Application.Interfaces;
using KuendaFinance.Operations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KuendaFinance.Operations.Infrastructure.Services;

public class ReportingService : IReportingService
{
    private readonly OperationsDbContext _context;

    public ReportingService(OperationsDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardMetricsDto> GetDashboardMetricsAsync(Guid tenantId, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var totalClients = await _context.Clients
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || c.BranchId == branchId.Value))
            .CountAsync(cancellationToken);

        var activeClients = await _context.Clients
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || c.BranchId == branchId.Value) &&
                        (c.Status == "Em Dia" || c.Status == "Atrasado" || c.Status == "Active" || c.Status == "Late"))
            .CountAsync(cancellationToken);

        var atRiskClients = await _context.Clients
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || c.BranchId == branchId.Value) &&
                        (c.Status == "Atrasado" || c.Status == "Late"))
            .CountAsync(cancellationToken);

        var activeLoans = await _context.Loans
            .IgnoreQueryFilters()
            .Include(l => l.Installments)
            .Where(l => l.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || l.BranchId == branchId.Value) &&
                        (l.Status == "Active" || l.Status == "Late"))
            .ToListAsync(cancellationToken);

        var totalActivePortfolio = activeLoans.Sum(l => l.TotalToPay - l.PaidAmount);

        var dueTodayCount = activeLoans
            .SelectMany(l => l.Installments)
            .Count(i => i.DueDate.Date == DateTime.UtcNow.Date && i.Status != "Paid");

        decimal par30Rate = 0m;
        if (activeLoans.Any())
        {
            var lateCount = activeLoans.Count(l => l.Status == "Late");
            par30Rate = Math.Round(((decimal)lateCount / activeLoans.Count) * 100, 1);
        }

        // Cash KPIs
        var baseBalance = 500000m;
        var inflows = await _context.Transactions
            .IgnoreQueryFilters()
            .Where(t => t.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || t.BranchId == branchId.Value) && t.Type == "Entrada")
            .SumAsync(t => t.Amount, cancellationToken);

        var outflows = await _context.Transactions
            .IgnoreQueryFilters()
            .Where(t => t.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || t.BranchId == branchId.Value) && t.Type == "Saída")
            .SumAsync(t => t.Amount, cancellationToken);

        var cashBalance = baseBalance + inflows - outflows;

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var monthlyInflow = await _context.Transactions
            .IgnoreQueryFilters()
            .Where(t => t.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || t.BranchId == branchId.Value) &&
                        t.Type == "Entrada" && t.TransactionDate >= startOfMonth)
            .SumAsync(t => t.Amount, cancellationToken);

        var monthlyOutflow = await _context.Transactions
            .IgnoreQueryFilters()
            .Where(t => t.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || t.BranchId == branchId.Value) &&
                        t.Type == "Saída" && t.TransactionDate >= startOfMonth)
            .SumAsync(t => t.Amount, cancellationToken);

        return new DashboardMetricsDto
        {
            TotalClients = totalClients,
            ActiveClients = activeClients,
            AtRiskClients = atRiskClients,
            TotalActivePortfolio = totalActivePortfolio,
            DueTodayCount = dueTodayCount,
            Par30Rate = par30Rate,
            CashBalance = cashBalance,
            MonthlyInflow = monthlyInflow,
            MonthlyOutflow = monthlyOutflow
        };
    }

    public async Task<List<Map10ItemDto>> GetMAP10ReportAsync(Guid tenantId, Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var baseBalance = 500000m;
        
        var inflows = await _context.Transactions
            .IgnoreQueryFilters()
            .Where(t => t.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || t.BranchId == branchId.Value) && 
                        t.Type == "Entrada" && t.TransactionDate <= endDate)
            .SumAsync(t => t.Amount, cancellationToken);

        var outflows = await _context.Transactions
            .IgnoreQueryFilters()
            .Where(t => t.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || t.BranchId == branchId.Value) && 
                        t.Type == "Saída" && t.TransactionDate <= endDate)
            .SumAsync(t => t.Amount, cancellationToken);

        var cashBalance = baseBalance + inflows - outflows;

        var activeLoans = await _context.Loans
            .IgnoreQueryFilters()
            .Where(l => l.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || l.BranchId == branchId.Value) &&
                        l.DisbursedAt != null && l.DisbursedAt <= endDate && l.Status != "Paid")
            .ToListAsync(cancellationToken);

        var activePortfolio = activeLoans.Sum(l => l.TotalToPay - l.PaidAmount);
        var totalInterest = activeLoans.Sum(l => l.TotalToPay - l.Amount);

        var results = new List<Map10ItemDto>
        {
            new() { AccountCode = "11.100.00", AccountDescription = "Disponibilidades (Caixa e Bancos)", Debit = cashBalance, Credit = 0m },
            new() { AccountCode = "14.200.00", AccountDescription = "Carteira de Crédito Bruta", Debit = activePortfolio, Credit = 0m },
            new() { AccountCode = "14.900.00", AccountDescription = "Juros a Receber de Clientes", Debit = totalInterest, Credit = 0m },
            new() { AccountCode = "21.100.00", AccountDescription = "Capital Social Autorizado", Debit = 0m, Credit = baseBalance },
            new() { AccountCode = "29.100.00", AccountDescription = "Resultados Transitados (Lucro/Prejuízo)", Debit = 0m, Credit = Math.Max(0m, (cashBalance + activePortfolio + totalInterest) - baseBalance) }
        };

        return results;
    }

    public async Task<List<Map14ItemDto>> GetMAP14ReportAsync(Guid tenantId, Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var activeLoans = await _context.Loans
            .IgnoreQueryFilters()
            .Where(l => l.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || l.BranchId == branchId.Value) &&
                        l.Amount >= 15000m && l.DisbursedAt != null && l.DisbursedAt <= endDate)
            .ToListAsync(cancellationToken);

        var clients = await _context.Clients
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || c.BranchId == branchId.Value))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var equity = 500000m;

        return activeLoans.Select(l => new Map14ItemDto
        {
            ContractId = l.Id.ToString().Substring(0, 8).ToUpper(),
            ClientName = clients.TryGetValue(l.ClientId, out var name) ? name : "Cliente Desconhecido",
            LoanAmount = l.Amount,
            ExposureRatio = Math.Round((l.Amount / equity) * 100, 1)
        }).ToList();
    }

    public async Task<List<ActivePortfolioItemDto>> GetActivePortfolioReportAsync(Guid tenantId, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var activeLoans = await _context.Loans
            .IgnoreQueryFilters()
            .Where(l => l.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || l.BranchId == branchId.Value) &&
                        (l.Status == "Active" || l.Status == "Late"))
            .ToListAsync(cancellationToken);

        var clients = await _context.Clients
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || c.BranchId == branchId.Value))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        return activeLoans.Select(l => new ActivePortfolioItemDto
        {
            ContractId = l.Id.ToString().Substring(0, 8).ToUpper(),
            ClientName = clients.TryGetValue(l.ClientId, out var name) ? name : "Cliente Desconhecido",
            Amount = l.Amount,
            InterestRate = l.InterestRate,
            TermMonths = l.TermMonths,
            PaidAmount = l.PaidAmount,
            UnpaidBalance = l.TotalToPay - l.PaidAmount,
            Status = l.Status
        }).ToList();
    }

    public async Task<List<Par30ItemDto>> GetPAR30ReportAsync(Guid tenantId, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var lateLoans = await _context.Loans
            .IgnoreQueryFilters()
            .Where(l => l.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || l.BranchId == branchId.Value) &&
                        l.Status == "Late")
            .ToListAsync(cancellationToken);

        var clients = await _context.Clients
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || c.BranchId == branchId.Value))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        return lateLoans.Select(l => new Par30ItemDto
        {
            ContractId = l.Id.ToString().Substring(0, 8).ToUpper(),
            ClientName = clients.TryGetValue(l.ClientId, out var name) ? name : "Cliente Desconhecido",
            Amount = l.Amount,
            UnpaidBalance = l.TotalToPay - l.PaidAmount,
            NextPaymentDate = l.NextPaymentDate,
            Status = l.Status
        }).ToList();
    }

    public async Task<List<DisbursementReportItemDto>> GetDisbursementsReportAsync(Guid tenantId, Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var disbursedLoans = await _context.Loans
            .IgnoreQueryFilters()
            .Where(l => l.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || l.BranchId == branchId.Value) &&
                        l.DisbursedAt >= startDate && l.DisbursedAt <= endDate)
            .ToListAsync(cancellationToken);

        var clients = await _context.Clients
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || c.BranchId == branchId.Value))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        return disbursedLoans.Select(l => new DisbursementReportItemDto
        {
            ContractId = l.Id.ToString().Substring(0, 8).ToUpper(),
            ClientName = clients.TryGetValue(l.ClientId, out var name) ? name : "Cliente Desconhecido",
            DisbursementDate = l.DisbursedAt ?? DateTime.UtcNow,
            PrincipalAmount = l.Amount,
            TermMonths = l.TermMonths,
            DisbursementMethod = l.DisbursementMethod ?? "Banco"
        }).ToList();
    }

    public async Task<List<CashFlowReportItemDto>> GetCashFlowReportAsync(Guid tenantId, Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var transactions = await _context.Transactions
            .IgnoreQueryFilters()
            .Where(t => t.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || t.BranchId == branchId.Value) &&
                        t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);

        return transactions.Select(t => new CashFlowReportItemDto
        {
            TransactionId = t.Id.ToString().Substring(0, 8).ToUpper(),
            Date = t.TransactionDate,
            Description = t.Description,
            Category = t.Category,
            Type = t.Type,
            Amount = t.Amount
        }).ToList();
    }

    public async Task<ProfitAndLossReportDto> GetPLReportAsync(Guid tenantId, Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var transactions = await _context.Transactions
            .IgnoreQueryFilters()
            .Where(t => t.TenantId == tenantId && (!branchId.HasValue || branchId == Guid.Empty || t.BranchId == branchId.Value) &&
                        t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .ToListAsync(cancellationToken);

        var interestInflow = transactions
            .Where(t => t.Category == "Receita Juros" || t.Category == "Amortização")
            .Sum(t => t.Amount);

        var salaries = transactions.Where(t => t.Category == "Salários").Sum(t => t.Amount);
        var rents = transactions.Where(t => t.Category == "Renda").Sum(t => t.Amount);
        var utilities = transactions.Where(t => t.Category == "Energia" || t.Category == "Água").Sum(t => t.Amount);

        return new ProfitAndLossReportDto
        {
            InterestInflow = interestInflow,
            SalariesOutflow = salaries,
            RentOutflow = rents,
            UtilitiesOutflow = utilities,
            NetProfit = interestInflow - (salaries + rents + utilities)
        };
    }
}
