using System;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Domain.Entities;
using KuendaFinance.Operations.Domain.Repositories;
using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Queries.CreditSettings;

public record GetCreditSettingsQuery(Guid TenantId) : IRequest<Result<CreditSettingsDto>>;

public class GetCreditSettingsQueryHandler : IRequestHandler<GetCreditSettingsQuery, Result<CreditSettingsDto>>
{
    private readonly ICreditSettingsRepository _repository;

    public GetCreditSettingsQueryHandler(ICreditSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CreditSettingsDto>> Handle(GetCreditSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (settings == null)
        {
            // Fallback to default rules
            return Result.Success(new CreditSettingsDto
            {
                TenantId = request.TenantId,
                DefaultInterestRate = 5.0m,
                DefaultPenaltyRate = 1.0m,
                OriginationFee = 500.0m,
                MaxTermMonths = 24,
                Currency = "MZN"
            });
        }

        return Result.Success(new CreditSettingsDto
        {
            TenantId = settings.TenantId,
            DefaultInterestRate = settings.DefaultInterestRate,
            DefaultPenaltyRate = settings.DefaultPenaltyRate,
            OriginationFee = settings.OriginationFee,
            MaxTermMonths = settings.MaxTermMonths,
            Currency = settings.Currency
        });
    }
}
