using KuendaFinance.IAM.Application.DTOs;
using KuendaFinance.IAM.Domain.Entities;
using KuendaFinance.IAM.Domain.Repositories;
using KuendaFinance.IAM.Domain.Authentication;
using MediatR;

namespace KuendaFinance.IAM.Application.Commands.RegisterUser;

public record RegisterUserCommand(string Email, string Password, string FirstName, string LastName) : IRequest<AuthResultDto>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterUserCommandHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResultDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate if user exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is null == false)
        {
            throw new Exception("User with this email already exists.");
        }

        // 2. Create User Entity
        var user = new User(Guid.NewGuid(), Guid.Empty, request.Email, request.FirstName, request.LastName);

        // 3. Persist User
        await _userRepository.AddAsync(user, request.Password, cancellationToken);

        // 4. Generate Token (default role 'User' for now)
        var token = _jwtTokenGenerator.GenerateToken(user, "system", new[] { "User" });

        // 5. Return result
        var branchRoles = new List<UserBranchRoleDto>();
        var userDto = new UserDto(user.Id, user.TenantId, user.Email, user.FirstName, user.LastName, user.IsActive, branchRoles);
        var tenantDto = new TenantDto(user.TenantId, "system", "System Tenant", "#6366f1", "#4f46e5");
        return new AuthResultDto(token, userDto, tenantDto);
    }
}
