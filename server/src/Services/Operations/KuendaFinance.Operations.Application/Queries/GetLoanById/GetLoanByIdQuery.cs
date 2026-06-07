using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.DTOs;
using KuendaFinance.Operations.Domain.Repositories;
using MediatR;
using KuendaFinance.Shared.Results;

namespace KuendaFinance.Operations.Application.Queries.GetLoanById;

public record GetLoanByIdQuery(Guid Id) : IRequest<Result<LoanDto>>;

public class GetLoanByIdQueryHandler : IRequestHandler<GetLoanByIdQuery, Result<LoanDto>>
{
    private readonly ILoanRepository _loanRepository;

    public GetLoanByIdQueryHandler(ILoanRepository loanRepository)
    {
        _loanRepository = loanRepository;
    }

    public async Task<Result<LoanDto>> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
    {
        var loan = await _loanRepository.GetByIdAsync(request.Id, cancellationToken);
        if (loan == null)
        {
            return Result.Failure<LoanDto>(new Error("Loan.NotFound", $"Loan with ID '{request.Id}' was not found."));
        }

        var dto = loan.ToDto();
        return Result.Success(dto);
    }
}
