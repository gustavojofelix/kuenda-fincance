using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using KuendaFinance.IAM.Application.DTOs;
using KuendaFinance.IAM.Domain.Entities;
using KuendaFinance.IAM.Domain.Repositories;
using KuendaFinance.Shared.Messaging;
using KuendaFinance.Shared.Results;
using MediatR;

namespace KuendaFinance.IAM.Application.Queries.Branches;

public record GetBranchesQuery(Guid TenantId) : IRequest<Result<List<BranchDto>>>;

public class GetBranchesQueryHandler : IRequestHandler<GetBranchesQuery, Result<List<BranchDto>>>
{
    private readonly IBranchRepository _repository;

    public GetBranchesQueryHandler(IBranchRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<BranchDto>>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var branches = await _repository.GetBranchesByTenantIdAsync(request.TenantId, cancellationToken);
        var dtos = branches.Select(b => new BranchDto
        {
            Id = b.Id,
            TenantId = b.TenantId,
            Name = b.Name,
            Cellphone = b.Cellphone,
            Email = b.Email,
            City = b.City,
            Address = b.Address,
            Manager = b.Manager,
            Status = b.Status
        }).ToList();

        return Result.Success(dtos);
    }
}

public record CreateBranchCommand(
    Guid TenantId,
    string Name,
    string Cellphone,
    string Email,
    string City,
    string Address,
    string Manager,
    string Status
) : ICommand<BranchDto>;

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3);
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.Address).NotEmpty();
        RuleFor(x => x.Manager).NotEmpty();
        RuleFor(x => x.Cellphone).NotEmpty().MinimumLength(9);
    }
}

public class CreateBranchCommandHandler : ICommandHandler<CreateBranchCommand, BranchDto>
{
    private readonly IBranchRepository _repository;

    public CreateBranchCommandHandler(IBranchRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<BranchDto>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = new Branch(
            Guid.NewGuid(),
            request.TenantId,
            request.Name,
            request.Cellphone,
            request.Email,
            request.City,
            request.Address,
            request.Manager,
            request.Status
        );

        await _repository.AddAsync(branch, cancellationToken);

        var dto = new BranchDto
        {
            Id = branch.Id,
            TenantId = branch.TenantId,
            Name = branch.Name,
            Cellphone = branch.Cellphone,
            Email = branch.Email,
            City = branch.City,
            Address = branch.Address,
            Manager = branch.Manager,
            Status = branch.Status
        };

        return Result.Success(dto);
    }
}

public record UpdateBranchCommand(
    Guid BranchId,
    Guid TenantId,
    string Name,
    string Cellphone,
    string Email,
    string City,
    string Address,
    string Manager,
    string Status
) : ICommand<BranchDto>;

public class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
{
    public UpdateBranchCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3);
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.Address).NotEmpty();
        RuleFor(x => x.Manager).NotEmpty();
        RuleFor(x => x.Cellphone).NotEmpty().MinimumLength(9);
    }
}

public class UpdateBranchCommandHandler : ICommandHandler<UpdateBranchCommand, BranchDto>
{
    private readonly IBranchRepository _repository;

    public UpdateBranchCommandHandler(IBranchRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<BranchDto>> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _repository.GetByIdAsync(request.BranchId, cancellationToken);
        if (branch == null || branch.TenantId != request.TenantId)
        {
            return Result.Failure<BranchDto>(new Error("Branch.NotFound", "Branch not found or access denied."));
        }

        branch.UpdateDetails(
            request.Name,
            request.Cellphone,
            request.Email,
            request.City,
            request.Address,
            request.Manager,
            request.Status
        );

        await _repository.UpdateAsync(branch, cancellationToken);

        var dto = new BranchDto
        {
            Id = branch.Id,
            TenantId = branch.TenantId,
            Name = branch.Name,
            Cellphone = branch.Cellphone,
            Email = branch.Email,
            City = branch.City,
            Address = branch.Address,
            Manager = branch.Manager,
            Status = branch.Status
        };

        return Result.Success(dto);
    }
}

public record DeleteBranchCommand(Guid BranchId, Guid TenantId) : IRequest<Result>;

public class DeleteBranchCommandHandler : IRequestHandler<DeleteBranchCommand, Result>
{
    private readonly IBranchRepository _repository;

    public DeleteBranchCommandHandler(IBranchRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _repository.GetByIdAsync(request.BranchId, cancellationToken);
        if (branch == null || branch.TenantId != request.TenantId)
        {
            return Result.Failure(new Error("Branch.NotFound", "Branch not found or access denied."));
        }

        await _repository.DeleteAsync(branch, cancellationToken);
        return Result.Success();
    }
}
