using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Domain.Repositories;
using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Queries.GetTransactions;

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

public record GetTransactionsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string SearchTerm = "",
    string Category = "",
    string Type = "",
    Guid? BranchId = null
) : IRequest<Result<PagedResult<TransactionDto>>>;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, Result<PagedResult<TransactionDto>>>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionsQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<PagedResult<TransactionDto>>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var (transactions, totalCount) = await _transactionRepository.GetTransactionsPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.Category,
            request.Type,
            request.BranchId,
            cancellationToken
        );

        var items = transactions.Select(tx => tx.ToDto()).ToList();

        var result = new PagedResult<TransactionDto>(items, totalCount, request.PageNumber, request.PageSize);
        return Result.Success(result);
    }
}
