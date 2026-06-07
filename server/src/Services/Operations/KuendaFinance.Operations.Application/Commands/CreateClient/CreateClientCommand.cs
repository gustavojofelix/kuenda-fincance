using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Domain.Entities;
using KuendaFinance.Operations.Domain.Repositories;
using KuendaFinance.Shared.Messaging;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Commands.CreateClient;

public record CreateGuaranteeInput(
    string Name,
    decimal Value,
    string PhotoUrl
);

public record CreateClientCommand(
    Guid TenantId,
    Guid BranchId,
    string Name,
    string BI,
    string Phone,
    string MaritalStatus,
    string Province,
    string District,
    string Neighborhood,
    string Address,
    string Business,
    string BusinessYears,
    string Income,
    string EmergencyName,
    string EmergencyRelation,
    string EmergencyPhone,
    List<CreateGuaranteeInput> Guarantees
) : ICommand<ClientDto>;

public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        
        RuleFor(x => x.BI)
            .NotEmpty().WithMessage("Identity card (BI) is required.")
            .Matches(@"^\d{12}[A-Za-z]$").WithMessage("Invalid Mozambique BI format. Should be 12 digits followed by a letter (e.g. 123456789012A).");
            
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^(\+258)?(8[234567]\d{7})$").WithMessage("Invalid Mozambique phone number format. Must start with +258 or directly with 82/83/84/85/86/87 followed by 7 digits.");
            
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId is required.");
        RuleFor(x => x.BranchId).NotEmpty().WithMessage("BranchId is required.");
    }
}

public class CreateClientCommandHandler : ICommandHandler<CreateClientCommand, ClientDto>
{
    private readonly IClientRepository _clientRepository;

    public CreateClientCommandHandler(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<Result<ClientDto>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        // Check if client with same BI already exists in the same tenant
        var existingClient = await _clientRepository.GetByBiAsync(request.BI, request.TenantId, cancellationToken);
        if (existingClient != null)
        {
            return Result.Failure<ClientDto>(new Error("Client.DuplicateBi", "A client with this BI number already exists under this Tenant."));
        }

        var client = new Client(Guid.NewGuid())
        {
            TenantId = request.TenantId,
            BranchId = request.BranchId,
            Name = request.Name,
            BI = request.BI,
            Phone = request.Phone,
            MaritalStatus = request.MaritalStatus,
            Province = request.Province,
            District = request.District,
            Neighborhood = request.Neighborhood,
            Address = request.Address,
            Business = request.Business,
            BusinessYears = request.BusinessYears,
            Income = request.Income,
            EmergencyName = request.EmergencyName,
            EmergencyRelation = request.EmergencyRelation,
            EmergencyPhone = request.EmergencyPhone,
            Status = "Evaluation",
            LoanCycle = 0
        };

        if (request.Guarantees != null)
        {
            foreach (var g in request.Guarantees)
            {
                client.Guarantees.Add(new Guarantee(Guid.NewGuid())
                {
                    ClientId = client.Id,
                    Name = g.Name,
                    Value = g.Value,
                    PhotoUrl = g.PhotoUrl
                });
            }
        }

        await _clientRepository.AddAsync(client, cancellationToken);

        // Add client status log entry
        var statusLog = new ClientStatusHistory(Guid.NewGuid())
        {
            ClientId = client.Id,
            PreviousStatus = "None",
            NewStatus = client.Status,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = client.CreatedBy ?? "system",
            Notes = "Client registered in system (Evaluation mode)."
        };
        await _clientRepository.AddStatusHistoryAsync(statusLog, cancellationToken);

        var dto = new ClientDto
        {
            Id = client.Id,
            TenantId = client.TenantId,
            BranchId = client.BranchId,
            Name = client.Name,
            BI = client.BI,
            Phone = client.Phone,
            MaritalStatus = client.MaritalStatus,
            Province = client.Province,
            District = client.District,
            Neighborhood = client.Neighborhood,
            Address = client.Address,
            Business = client.Business,
            BusinessYears = client.BusinessYears,
            Income = client.Income,
            EmergencyName = client.EmergencyName,
            EmergencyRelation = client.EmergencyRelation,
            EmergencyPhone = client.EmergencyPhone,
            Status = client.Status,
            LoanCycle = client.LoanCycle,
            CreatedAt = client.CreatedAt,
            CreatedBy = client.CreatedBy,
            LastUpdated = client.LastUpdated,
            UpdatedBy = client.UpdatedBy,
            Guarantees = client.Guarantees.Select(g => new GuaranteeDto
            {
                Id = g.Id,
                ClientId = g.ClientId,
                Name = g.Name,
                Value = g.Value,
                PhotoUrl = g.PhotoUrl
            }).ToList()
        };

        return Result.Success(dto);
    }
}
