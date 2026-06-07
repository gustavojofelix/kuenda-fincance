using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.IAM.Application.DTOs;
using KuendaFinance.IAM.Domain.Authentication;
using KuendaFinance.IAM.Domain.Entities;
using KuendaFinance.IAM.Domain.Repositories;
using MediatR;

namespace KuendaFinance.IAM.Application.Commands.RegisterImf;

public record RegisterImfCommand(
    string ImfName,
    string Nuit,
    string AdminName,
    string AdminEmail,
    string AdminCellphone,
    string Password,
    string Province,
    string City,
    string FullAddress
) : IRequest<AuthResultDto>;

public class RegisterImfCommandHandler : IRequestHandler<RegisterImfCommand, AuthResultDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserBranchRoleRepository _userBranchRoleRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ITransactionManager _transactionManager;

    public RegisterImfCommandHandler(
        ITenantRepository tenantRepository,
        IBranchRepository branchRepository,
        IUserRepository userRepository,
        IUserBranchRoleRepository userBranchRoleRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        ITransactionManager transactionManager)
    {
        _tenantRepository = tenantRepository;
        _branchRepository = branchRepository;
        _userRepository = userRepository;
        _userBranchRoleRepository = userBranchRoleRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _transactionManager = transactionManager;
    }

    public async Task<AuthResultDto> Handle(RegisterImfCommand request, CancellationToken cancellationToken)
    {
        return await _transactionManager.ExecuteAsync(async () =>
        {
            // 1. Generate Unique IMF Code
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random();
            var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string code = "";
            bool codeExists = true;
            
            while (codeExists)
            {
                var r1 = alphabet[random.Next(alphabet.Length)];
                var r2 = alphabet[random.Next(alphabet.Length)];
                code = $"IMF-{today}{r1}{r2}";
                codeExists = await _tenantRepository.CodeExistsAsync(code, cancellationToken);
            }

            // 2. Select Theme Colors Randomly
            var colorThemes = new[]
            {
                new { Primary = "#10b981", Secondary = "#059669" }, // Emerald Green
                new { Primary = "#2563eb", Secondary = "#1d4ed8" }, // Royal Blue
                new { Primary = "#6366f1", Secondary = "#4f46e5" }, // Indigo
                new { Primary = "#8b5cf6", Secondary = "#7c3aed" }, // Purple
                new { Primary = "#ec4899", Secondary = "#db2777" }, // Pink
                new { Primary = "#06b6d4", Secondary = "#0891b2" }, // Cyan
                new { Primary = "#f59e0b", Secondary = "#d97706" }  // Amber
            };
            var selectedTheme = colorThemes[random.Next(colorThemes.Length)];

            // 3. Create Tenant (Subdomain is lowercase of the generated IMF code)
            var tenantId = Guid.NewGuid();
            var subdomain = code.ToLowerInvariant();
            var tenant = new Tenant(
                tenantId,
                code,
                request.ImfName,
                subdomain,
                request.Nuit,
                request.AdminEmail,
                request.AdminCellphone,
                request.FullAddress,
                selectedTheme.Primary,
                selectedTheme.Secondary,
                "Standard"
            );
            await _tenantRepository.AddAsync(tenant, cancellationToken);

            // 4. Create Default Agency/Branch (Agência Sede)
            var branchId = Guid.NewGuid();
            var branch = new Branch(
                branchId,
                tenantId,
                "Agência Sede",
                request.AdminCellphone,
                request.AdminEmail,
                request.City,
                request.FullAddress,
                request.AdminName,
                "Active"
            );
            await _branchRepository.AddAsync(branch, cancellationToken);

            // 5. Create Admin User
            var parts = request.AdminName.Trim().Split(' ');
            string firstName = parts[0];
            string lastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty;

            var userId = Guid.NewGuid();
            var user = new User(
                userId,
                tenantId,
                request.AdminEmail,
                firstName,
                lastName
            );
            await _userRepository.AddAsync(user, request.Password, cancellationToken);

            // 6. Allocate Admin User to Branch with Role "Administrator"
            var userBranchRoleId = Guid.NewGuid();
            var userBranchRole = new UserBranchRole(
                userBranchRoleId,
                userId,
                branchId,
                "Administrator"
            );
            await _userBranchRoleRepository.AddAsync(userBranchRole, cancellationToken);

            // 7. Generate Token
            var token = _jwtTokenGenerator.GenerateToken(user, code, new[] { "Admin" });

            // 8. Return
            var branchRoles = new List<UserBranchRoleDto>
            {
                new UserBranchRoleDto(branchId, branch.Name, "Administrator")
            };
            var userDto = new UserDto(userId, tenantId, user.Email, user.FirstName, user.LastName, user.IsActive, branchRoles);
            var tenantDto = new TenantDto(tenantId, code, tenant.Name, tenant.PrimaryColor, tenant.SecondaryColor);

            return new AuthResultDto(token, userDto, tenantDto);
        }, cancellationToken);
    }
}
