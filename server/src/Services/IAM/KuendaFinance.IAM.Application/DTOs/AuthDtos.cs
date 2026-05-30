namespace KuendaFinance.IAM.Application.DTOs;

public record AuthResultDto(string Token, UserDto User);

public record UserDto(Guid Id, string Email, string FirstName, string LastName, bool IsActive);
