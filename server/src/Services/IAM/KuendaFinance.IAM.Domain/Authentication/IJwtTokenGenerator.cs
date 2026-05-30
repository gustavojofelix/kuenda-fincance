using KuendaFinance.IAM.Domain.Entities;

namespace KuendaFinance.IAM.Domain.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, IEnumerable<string> roles);
}
