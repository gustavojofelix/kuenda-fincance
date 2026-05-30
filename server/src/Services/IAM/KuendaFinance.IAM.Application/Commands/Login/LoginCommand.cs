using KuendaFinance.IAM.Application.DTOs;
using KuendaFinance.IAM.Domain.Repositories;
using KuendaFinance.IAM.Domain.Authentication;
using MediatR;

namespace KuendaFinance.IAM.Application.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResultDto>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Get User
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            throw new Exception("Invalid email or password.");
        }

        // 2. Check Password
        var isPasswordValid = await _userRepository.CheckPasswordAsync(user, request.Password, cancellationToken);
        if (!isPasswordValid)
        {
            throw new Exception("Invalid email or password.");
        }

        // 3. Generate Token
        // Ideally we should fetch user roles from a repository
        var token = _jwtTokenGenerator.GenerateToken(user, new[] { "User" });

        // 4. Return
        var userDto = new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.IsActive);
        return new AuthResultDto(token, userDto);
    }
}
