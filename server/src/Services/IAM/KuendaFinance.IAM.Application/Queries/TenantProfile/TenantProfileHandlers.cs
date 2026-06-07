using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using KuendaFinance.IAM.Application.DTOs;
using KuendaFinance.IAM.Domain.Repositories;
using KuendaFinance.Shared.Messaging;
using KuendaFinance.Shared.Results;
using MediatR;

namespace KuendaFinance.IAM.Application.Queries.TenantProfile;

public record GetTenantProfileQuery(Guid TenantId) : IRequest<Result<TenantProfileDto>>;

public class GetTenantProfileQueryHandler : IRequestHandler<GetTenantProfileQuery, Result<TenantProfileDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantProfileQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<TenantProfileDto>> Handle(GetTenantProfileQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            return Result.Failure<TenantProfileDto>(new Error("Tenant.NotFound", "Tenant not found."));
        }

        return Result.Success(new TenantProfileDto
        {
            Id = tenant.Id,
            Code = tenant.Code,
            Name = tenant.Name,
            Subdomain = tenant.Subdomain,
            Nuit = tenant.Nuit,
            Email = tenant.Email,
            Phone = tenant.Phone,
            Address = tenant.Address,
            PrimaryColor = tenant.PrimaryColor,
            SecondaryColor = tenant.SecondaryColor,
            LogoUrl = tenant.LogoUrl
        });
    }
}

public record UpdateTenantBrandingCommand(
    Guid TenantId,
    string Name,
    string Email,
    string Phone,
    string Address,
    string PrimaryColor,
    string SecondaryColor,
    string? LogoUrl
) : ICommand<TenantProfileDto>;

public class UpdateTenantBrandingCommandValidator : AbstractValidator<UpdateTenantBrandingCommand>
{
    public UpdateTenantBrandingCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tenant name is required.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Valid email is required.");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone is required.");
        RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required.");
        RuleFor(x => x.PrimaryColor).NotEmpty().WithMessage("Primary color is required.");
        RuleFor(x => x.SecondaryColor).NotEmpty().WithMessage("Secondary color is required.");
    }
}

public class UpdateTenantBrandingCommandHandler : ICommandHandler<UpdateTenantBrandingCommand, TenantProfileDto>
{
    private readonly ITenantRepository _tenantRepository;

    public UpdateTenantBrandingCommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<TenantProfileDto>> Handle(UpdateTenantBrandingCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            return Result.Failure<TenantProfileDto>(new Error("Tenant.NotFound", "Tenant not found."));
        }

        tenant.UpdateBranding(
            request.Name,
            request.Email,
            request.Phone,
            request.Address,
            request.PrimaryColor,
            request.SecondaryColor,
            request.LogoUrl
        );

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);

        return Result.Success(new TenantProfileDto
        {
            Id = tenant.Id,
            Code = tenant.Code,
            Name = tenant.Name,
            Subdomain = tenant.Subdomain,
            Nuit = tenant.Nuit,
            Email = tenant.Email,
            Phone = tenant.Phone,
            Address = tenant.Address,
            PrimaryColor = tenant.PrimaryColor,
            SecondaryColor = tenant.SecondaryColor,
            LogoUrl = tenant.LogoUrl
        });
    }
}
