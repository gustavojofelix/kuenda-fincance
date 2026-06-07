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

namespace KuendaFinance.IAM.Application.Queries.Team;

public record GetTeamMembersQuery(Guid TenantId) : IRequest<Result<List<TeamUserDto>>>;

public class GetTeamMembersQueryHandler : IRequestHandler<GetTeamMembersQuery, Result<List<TeamUserDto>>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserBranchRoleRepository _userBranchRoleRepository;
    private readonly IBranchRepository _branchRepository;

    public GetTeamMembersQueryHandler(
        IUserRepository userRepository,
        IUserBranchRoleRepository userBranchRoleRepository,
        IBranchRepository branchRepository)
    {
        _userRepository = userRepository;
        _userBranchRoleRepository = userBranchRoleRepository;
        _branchRepository = branchRepository;
    }

    public async Task<Result<List<TeamUserDto>>> Handle(GetTeamMembersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetTeamMembersAsync(request.TenantId, cancellationToken);
        var branches = await _branchRepository.GetBranchesByTenantIdAsync(request.TenantId, cancellationToken);
        var branchDict = branches.ToDictionary(b => b.Id, b => b.Name);

        var dtos = new List<TeamUserDto>();

        foreach (var u in users)
        {
            var branchRoles = await _userBranchRoleRepository.GetByUserIdAsync(u.Id, cancellationToken);
            
            dtos.Add(new TeamUserDto
            {
                Id = u.Id,
                Name = $"{u.FirstName} {u.LastName}".Trim(),
                Email = u.Email,
                Phone = u.Phone,
                Status = u.IsActive ? "Active" : "Inactive",
                BranchRoles = branchRoles.Select(br => new UserBranchRoleDto(
                    br.BranchId,
                    branchDict.TryGetValue(br.BranchId, out var bName) ? bName : "Agência Desconhecida",
                    br.Role
                )).ToList()
            });
        }

        return Result.Success(dtos);
    }
}

public record TeamBranchRoleInput(Guid BranchId, string Role);

public record InviteTeamMemberCommand(
    Guid TenantId,
    string Name,
    string Email,
    string? Phone,
    List<TeamBranchRoleInput> BranchRoles
) : ICommand<TeamUserDto>;

public class InviteTeamMemberCommandValidator : AbstractValidator<InviteTeamMemberCommand>
{
    public InviteTeamMemberCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.BranchRoles).NotEmpty().WithMessage("At least one branch role must be specified.");
    }
}

public class InviteTeamMemberCommandHandler : ICommandHandler<InviteTeamMemberCommand, TeamUserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserBranchRoleRepository _userBranchRoleRepository;
    private readonly IBranchRepository _branchRepository;

    public InviteTeamMemberCommandHandler(
        IUserRepository userRepository,
        IUserBranchRoleRepository userBranchRoleRepository,
        IBranchRepository branchRepository)
    {
        _userRepository = userRepository;
        _userBranchRoleRepository = userBranchRoleRepository;
        _branchRepository = branchRepository;
    }

    public async Task<Result<TeamUserDto>> Handle(InviteTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing != null)
        {
            return Result.Failure<TeamUserDto>(new Error("Team.UserExists", "A user with this email is already registered."));
        }

        var names = request.Name.Split(' ', 2);
        var firstName = names[0];
        var lastName = names.Length > 1 ? names[1] : string.Empty;

        var userId = Guid.NewGuid();
        var user = new User(
            userId,
            request.TenantId,
            request.Email,
            firstName,
            lastName,
            request.Phone
        );

        // Generate temporary password since it's required by Identity core setup
        var tempPassword = "TempPassword123!_" + Guid.NewGuid().ToString().Substring(0, 8);
        try
        {
            await _userRepository.AddAsync(user, tempPassword, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<TeamUserDto>(new Error("Team.CreateFailed", ex.Message));
        }

        var branches = await _branchRepository.GetBranchesByTenantIdAsync(request.TenantId, cancellationToken);
        var branchDict = branches.ToDictionary(b => b.Id, b => b.Name);

        var addedRoles = new List<UserBranchRoleDto>();

        foreach (var r in request.BranchRoles)
        {
            var ubr = new UserBranchRole(Guid.NewGuid(), userId, r.BranchId, r.Role);
            await _userBranchRoleRepository.AddAsync(ubr, cancellationToken);
            addedRoles.Add(new UserBranchRoleDto(
                r.BranchId,
                branchDict.TryGetValue(r.BranchId, out var bName) ? bName : "Agência Desconhecida",
                r.Role
            ));
        }

        var dto = new TeamUserDto
        {
            Id = user.Id,
            Name = request.Name,
            Email = user.Email,
            Phone = user.Phone,
            Status = "Pending", // Pending invite registration confirmation
            BranchRoles = addedRoles
        };

        return Result.Success(dto);
    }
}

public record UpdateTeamMemberCommand(
    Guid UserId,
    Guid TenantId,
    string Name,
    string? Phone,
    string Status,
    List<TeamBranchRoleInput> BranchRoles
) : ICommand<TeamUserDto>;

public class UpdateTeamMemberCommandValidator : AbstractValidator<UpdateTeamMemberCommand>
{
    public UpdateTeamMemberCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Status).NotEmpty();
        RuleFor(x => x.BranchRoles).NotEmpty().WithMessage("At least one branch role must be specified.");
    }
}

public class UpdateTeamMemberCommandHandler : ICommandHandler<UpdateTeamMemberCommand, TeamUserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserBranchRoleRepository _userBranchRoleRepository;
    private readonly IBranchRepository _branchRepository;

    public UpdateTeamMemberCommandHandler(
        IUserRepository userRepository,
        IUserBranchRoleRepository userBranchRoleRepository,
        IBranchRepository branchRepository)
    {
        _userRepository = userRepository;
        _userBranchRoleRepository = userBranchRoleRepository;
        _branchRepository = branchRepository;
    }

    public async Task<Result<TeamUserDto>> Handle(UpdateTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null || user.TenantId != request.TenantId)
        {
            return Result.Failure<TeamUserDto>(new Error("Team.UserNotFound", "Team member not found or access denied."));
        }

        var names = request.Name.Split(' ', 2);
        var firstName = names[0];
        var lastName = names.Length > 1 ? names[1] : string.Empty;
        
        user.UpdateProfile(firstName, lastName, request.Phone, user.PhotoUrl);

        if (request.Status == "Active" || request.Status == "Ativo")
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        try
        {
            await _userRepository.UpdateAsync(user, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<TeamUserDto>(new Error("Team.UpdateFailed", ex.Message));
        }

        // Fetch current branch roles and delete them
        var currentRoles = await _userBranchRoleRepository.GetByUserIdAsync(user.Id, cancellationToken);
        await _userBranchRoleRepository.RemoveRangeAsync(currentRoles, cancellationToken);

        var branches = await _branchRepository.GetBranchesByTenantIdAsync(request.TenantId, cancellationToken);
        var branchDict = branches.ToDictionary(b => b.Id, b => b.Name);

        var addedRoles = new List<UserBranchRoleDto>();

        foreach (var r in request.BranchRoles)
        {
            var ubr = new UserBranchRole(Guid.NewGuid(), user.Id, r.BranchId, r.Role);
            await _userBranchRoleRepository.AddAsync(ubr, cancellationToken);
            addedRoles.Add(new UserBranchRoleDto(
                r.BranchId,
                branchDict.TryGetValue(r.BranchId, out var bName) ? bName : "Agência Desconhecida",
                r.Role
            ));
        }

        var dto = new TeamUserDto
        {
            Id = user.Id,
            Name = request.Name,
            Email = user.Email,
            Phone = user.Phone,
            Status = user.IsActive ? "Active" : "Inactive",
            BranchRoles = addedRoles
        };

        return Result.Success(dto);
    }
}

public record DeleteTeamMemberCommand(Guid UserId, Guid TenantId) : IRequest<Result>;

public class DeleteTeamMemberCommandHandler : IRequestHandler<DeleteTeamMemberCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserBranchRoleRepository _userBranchRoleRepository;

    public DeleteTeamMemberCommandHandler(
        IUserRepository userRepository,
        IUserBranchRoleRepository userBranchRoleRepository)
    {
        _userRepository = userRepository;
        _userBranchRoleRepository = userBranchRoleRepository;
    }

    public async Task<Result> Handle(DeleteTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null || user.TenantId != request.TenantId)
        {
            return Result.Failure(new Error("Team.UserNotFound", "Team member not found or access denied."));
        }

        // Clean up branch roles
        var branchRoles = await _userBranchRoleRepository.GetByUserIdAsync(user.Id, cancellationToken);
        await _userBranchRoleRepository.RemoveRangeAsync(branchRoles, cancellationToken);

        try
        {
            await _userRepository.DeleteAsync(user, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Team.DeleteFailed", ex.Message));
        }

        return Result.Success();
    }
}
