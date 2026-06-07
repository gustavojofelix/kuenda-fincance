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

namespace KuendaFinance.Operations.Application.Commands.UpdateClient;

public record UpdateGuaranteeInput(
    Guid? Id,
    string Name,
    decimal Value,
    string PhotoUrl
);

public record UpdateClientCommand(
    Guid Id,
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
    string Status,
    List<UpdateGuaranteeInput> Guarantees
) : ICommand<ClientDto>;

public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("ClientId is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        
        RuleFor(x => x.BI)
            .NotEmpty().WithMessage("Identity card (BI) is required.")
            .Matches(@"^\d{12}[A-Za-z]$").WithMessage("Invalid Mozambique BI format. Should be 12 digits followed by a letter (e.g. 123456789012A).");
            
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^(\+258)?(8[234567]\d{7})$").WithMessage("Invalid Mozambique phone number format. Must start with +258 or directly with 82/83/84/85/86/87 followed by 7 digits.");
            
        RuleFor(x => x.Status).NotEmpty().WithMessage("Status is required.");
    }
}

public class UpdateClientCommandHandler : ICommandHandler<UpdateClientCommand, ClientDto>
{
    private readonly IClientRepository _clientRepository;

    public UpdateClientCommandHandler(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<Result<ClientDto>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.GetByIdAsync(request.Id, cancellationToken);
        if (client == null)
        {
            return Result.Failure<ClientDto>(new Error("Client.NotFound", $"Client with ID '{request.Id}' was not found."));
        }

        // Check multi-tenancy access boundary
        if (client.TenantId != request.TenantId)
        {
            return Result.Failure<ClientDto>(new Error("Client.Unauthorized", "You are not authorized to update this client."));
        }

        // Check BI duplicate (if BI changed)
        if (!string.Equals(client.BI, request.BI, StringComparison.OrdinalIgnoreCase))
        {
            var existingClient = await _clientRepository.GetByBiAsync(request.BI, request.TenantId, cancellationToken);
            if (existingClient != null)
            {
                return Result.Failure<ClientDto>(new Error("Client.DuplicateBi", "A client with this BI number already exists under this Tenant."));
            }
        }

        var oldStatus = client.Status;
        var newStatus = request.Status;

        // Update properties
        client.BranchId = request.BranchId;
        client.Name = request.Name;
        client.BI = request.BI;
        client.Phone = request.Phone;
        client.MaritalStatus = request.MaritalStatus;
        client.Province = request.Province;
        client.District = request.District;
        client.Neighborhood = request.Neighborhood;
        client.Address = request.Address;
        client.Business = request.Business;
        client.BusinessYears = request.BusinessYears;
        client.Income = request.Income;
        client.EmergencyName = request.EmergencyName;
        client.EmergencyRelation = request.EmergencyRelation;
        client.EmergencyPhone = request.EmergencyPhone;
        client.Status = request.Status;

        // Sync guarantees
        client.Guarantees.Clear();
        if (request.Guarantees != null)
        {
            foreach (var g in request.Guarantees)
            {
                client.Guarantees.Add(new Guarantee(g.Id ?? Guid.NewGuid())
                {
                    ClientId = client.Id,
                    Name = g.Name,
                    Value = g.Value,
                    PhotoUrl = g.PhotoUrl
                });
            }
        }

        await _clientRepository.UpdateAsync(client, cancellationToken);

        // Add status log if status changed
        if (!string.Equals(oldStatus, newStatus, StringComparison.OrdinalIgnoreCase))
        {
            var statusLog = new ClientStatusHistory(Guid.NewGuid())
            {
                ClientId = client.Id,
                PreviousStatus = oldStatus,
                NewStatus = newStatus,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = client.UpdatedBy ?? "system",
                Notes = $"Status updated from {oldStatus} to {newStatus}."
            };
            await _clientRepository.AddStatusHistoryAsync(statusLog, cancellationToken);
        }

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
