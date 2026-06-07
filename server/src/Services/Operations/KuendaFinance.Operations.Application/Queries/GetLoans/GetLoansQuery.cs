using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Domain.Repositories;
using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Queries.GetLoans;

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

public record GetLoansQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string Status = "",
    Guid? ClientId = null,
    Guid? BranchId = null
) : IRequest<Result<PagedResult<LoanDto>>>;

public class GetLoansQueryHandler : IRequestHandler<GetLoansQuery, Result<PagedResult<LoanDto>>>
{
    private readonly ILoanRepository _loanRepository;

    public GetLoansQueryHandler(ILoanRepository loanRepository)
    {
        _loanRepository = loanRepository;
    }

    public async Task<Result<PagedResult<LoanDto>>> Handle(GetLoansQuery request, CancellationToken cancellationToken)
    {
        var (loans, totalCount) = await _loanRepository.GetLoansPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.Status,
            request.ClientId,
            request.BranchId,
            cancellationToken
        );

        var items = loans.Select(loan => loan.ToDto()).ToList();

        var result = new PagedResult<LoanDto>(items, totalCount, request.PageNumber, request.PageSize);
        return Result.Success(result);
    }
}
