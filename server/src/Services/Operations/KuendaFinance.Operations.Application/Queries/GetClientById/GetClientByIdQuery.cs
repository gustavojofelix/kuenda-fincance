using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Domain.Repositories;
using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Queries.GetClientById;

public record GetClientByIdQuery(Guid Id) : IRequest<Result<ClientDto>>;

public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, Result<ClientDto>>
{
    private readonly IClientRepository _clientRepository;

    public GetClientByIdQueryHandler(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<Result<ClientDto>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.GetByIdAsync(request.Id, cancellationToken);

        if (client == null)
        {
            return Result.Failure<ClientDto>(new Error("Client.NotFound", $"Client with ID '{request.Id}' was not found."));
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
