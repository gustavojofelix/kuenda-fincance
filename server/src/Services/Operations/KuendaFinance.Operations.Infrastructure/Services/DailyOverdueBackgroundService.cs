using System;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.Commands.ProcessDailyPenalties;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KuendaFinance.Operations.Infrastructure.Services;

public class DailyOverdueBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailyOverdueBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1);

    public DailyOverdueBackgroundService(IServiceScopeFactory scopeFactory, ILogger<DailyOverdueBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily Overdue Checker & Penalty Background Service started.");

        // Initial delay to let the application start up fully before running the first check
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting scheduled daily overdue check and penalty calculation...");

                using (var scope = _scopeFactory.CreateScope())
                {
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var result = await mediator.Send(new ProcessDailyPenaltiesCommand(), stoppingToken);

                    if (result.IsSuccess)
                    {
                        _logger.LogInformation(
                            "Daily overdue check completed successfully. Processed {Loans} loans, penalized {Installments} installments, accrued {Amount} MZN in late penalties.",
                            result.Value.LoansProcessed,
                            result.Value.InstallmentsPenalized,
                            result.Value.TotalPenaltiesAccrued
                        );
                    }
                    else
                    {
                        _logger.LogWarning("Daily overdue check failed with error: {Error}", result.Error.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while running the daily overdue check background task.");
            }

            // Wait 24 hours before running the next check
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Daily Overdue Checker & Penalty Background Service is stopping.");
    }
}
