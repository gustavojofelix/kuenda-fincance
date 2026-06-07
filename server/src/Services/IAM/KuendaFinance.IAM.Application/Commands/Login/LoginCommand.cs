using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.IAM.Application.DTOs;
using KuendaFinance.IAM.Domain.Repositories;
using KuendaFinance.IAM.Domain.Authentication;
using MediatR;

namespace KuendaFinance.IAM.Application.Commands.Login;

public record LoginCommand(string ImfCode, string Email, string Password) : IRequest<AuthResultDto>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IUserBranchRoleRepository _userBranchRoleRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IBranchRepository branchRepository,
        IUserBranchRoleRepository userBranchRoleRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _branchRepository = branchRepository;
        _userBranchRoleRepository = userBranchRoleRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Get Tenant by IMF Code
        var tenant = await _tenantRepository.GetByCodeAsync(request.ImfCode, cancellationToken);
        if (tenant is null || !tenant.IsActive)
        {
            throw new Exception("Invalid IMF code, email or password.");
        }

        // 2. Get User
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !user.IsActive || user.TenantId != tenant.Id)
        {
            throw new Exception("Invalid IMF code, email or password.");
        }

        // 3. Check Password
        var isPasswordValid = await _userRepository.CheckPasswordAsync(user, request.Password, cancellationToken);
        if (!isPasswordValid)
        {
            throw new Exception("Invalid IMF code, email or password.");
        }

        // 4. Fetch Branch Roles for the User
        var userBranchRoles = await _userBranchRoleRepository.GetByUserIdAsync(user.Id, cancellationToken);
        var branchRoleDtos = new List<UserBranchRoleDto>();
        var rolesList = new List<string>();

        foreach (var ubr in userBranchRoles)
        {
            var branch = await _branchRepository.GetByIdAsync(ubr.BranchId, cancellationToken);
            var branchName = branch?.Name ?? "Unknown Branch";
            branchRoleDtos.Add(new UserBranchRoleDto(ubr.BranchId, branchName, ubr.Role));
            
            if (!rolesList.Contains(ubr.Role))
            {
                rolesList.Add(ubr.Role);
            }
        }

        if (!rolesList.Any())
        {
            rolesList.Add("User");
        }

        // 5. Generate Token
        var token = _jwtTokenGenerator.GenerateToken(user, tenant.Code, rolesList);

        // 6. Return payload
        var userDto = new UserDto(user.Id, user.TenantId, user.Email, user.FirstName, user.LastName, user.IsActive, branchRoleDtos);
        var tenantDto = new TenantDto(tenant.Id, tenant.Code, tenant.Name, tenant.PrimaryColor, tenant.SecondaryColor);

        return new AuthResultDto(token, userDto, tenantDto);
    }
}
