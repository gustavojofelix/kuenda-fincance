using System;
using System.Collections.Generic;

namespace KuendaFinance.IAM.Application.DTOs;

public record AuthResultDto(string Token, UserDto User, TenantDto Tenant);

public record UserDto(Guid Id, Guid TenantId, string Email, string FirstName, string LastName, bool IsActive, List<UserBranchRoleDto> BranchRoles);

public record TenantDto(Guid Id, string Code, string Name, string PrimaryColor, string SecondaryColor);

public record UserBranchRoleDto(Guid BranchId, string BranchName, string Role);
