using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Domain.Entities;
using KuendaFinance.Operations.Domain.Repositories;
using KuendaFinance.Shared.Messaging;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Commands.CreditSettings;

public record UpdateCreditSettingsCommand(
    Guid TenantId,
    decimal DefaultInterestRate,
    decimal DefaultPenaltyRate,
    decimal OriginationFee,
    int MaxTermMonths,
    string Currency
) : ICommand<CreditSettingsDto>;

public class UpdateCreditSettingsCommandValidator : AbstractValidator<UpdateCreditSettingsCommand>
{
    public UpdateCreditSettingsCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DefaultInterestRate).GreaterThanOrEqualTo(0).WithMessage("Default interest rate cannot be negative.");
        RuleFor(x => x.DefaultPenaltyRate).GreaterThanOrEqualTo(0).WithMessage("Default penalty rate cannot be negative.");
        RuleFor(x => x.OriginationFee).GreaterThanOrEqualTo(0).WithMessage("Origination fee cannot be negative.");
        RuleFor(x => x.MaxTermMonths).GreaterThan(0).WithMessage("Maximum term must be greater than zero.");
        RuleFor(x => x.Currency).NotEmpty().WithMessage("Currency is required.");
    }
}

public class UpdateCreditSettingsCommandHandler : ICommandHandler<UpdateCreditSettingsCommand, CreditSettingsDto>
{
    private readonly ICreditSettingsRepository _repository;

    public UpdateCreditSettingsCommandHandler(ICreditSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CreditSettingsDto>> Handle(UpdateCreditSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = new Domain.Entities.CreditSettings(Guid.NewGuid())
        {
            TenantId = request.TenantId,
            DefaultInterestRate = request.DefaultInterestRate,
            DefaultPenaltyRate = request.DefaultPenaltyRate,
            OriginationFee = request.OriginationFee,
            MaxTermMonths = request.MaxTermMonths,
            Currency = request.Currency
        };

        await _repository.SaveAsync(settings, cancellationToken);

        var dto = new CreditSettingsDto
        {
            TenantId = settings.TenantId,
            DefaultInterestRate = settings.DefaultInterestRate,
            DefaultPenaltyRate = settings.DefaultPenaltyRate,
            OriginationFee = settings.OriginationFee,
            MaxTermMonths = settings.MaxTermMonths,
            Currency = settings.Currency
        };

        return Result.Success(dto);
    }
}
