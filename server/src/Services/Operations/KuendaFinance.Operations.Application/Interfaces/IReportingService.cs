using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;

namespace KuendaFinance.Operations.Application.Interfaces;

public interface IReportingService
{
    Task<DashboardMetricsDto> GetDashboardMetricsAsync(Guid tenantId, Guid? branchId, CancellationToken cancellationToken = default);
    
    Task<List<Map10ItemDto>> GetMAP10ReportAsync(Guid tenantId, Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    Task<List<Map14ItemDto>> GetMAP14ReportAsync(Guid tenantId, Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    Task<List<ActivePortfolioItemDto>> GetActivePortfolioReportAsync(Guid tenantId, Guid? branchId, CancellationToken cancellationToken = default);
    
    Task<List<Par30ItemDto>> GetPAR30ReportAsync(Guid tenantId, Guid? branchId, CancellationToken cancellationToken = default);
    
    Task<List<DisbursementReportItemDto>> GetDisbursementsReportAsync(Guid tenantId, Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    Task<List<CashFlowReportItemDto>> GetCashFlowReportAsync(Guid tenantId, Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    Task<ProfitAndLossReportDto> GetPLReportAsync(Guid tenantId, Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
