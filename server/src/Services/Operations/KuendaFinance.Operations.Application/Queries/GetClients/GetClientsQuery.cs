using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Domain.Repositories;
using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Queries.GetClients;

public class PagedResult<T>
{
    public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public List<T> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public record GetClientsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string SearchTerm = "",
    string Province = "",
    string Status = "",
    Guid? BranchId = null
) : IRequest<Result<PagedResult<ClientDto>>>;

public class GetClientsQueryHandler : IRequestHandler<GetClientsQuery, Result<PagedResult<ClientDto>>>
{
    private readonly IClientRepository _clientRepository;

    public GetClientsQueryHandler(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<Result<PagedResult<ClientDto>>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
    {
        var (clients, totalCount) = await _clientRepository.GetClientsPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.Province,
            request.Status,
            request.BranchId,
            cancellationToken
        );

        var items = clients.Select(client => new ClientDto
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
        }).ToList();

        var result = new PagedResult<ClientDto>(items, totalCount, request.PageNumber, request.PageSize);
        return Result.Success(result);
    }
}
