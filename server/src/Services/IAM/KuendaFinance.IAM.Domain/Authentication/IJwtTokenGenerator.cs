using System.Collections.Generic;
using KuendaFinance.IAM.Domain.Entities;

namespace KuendaFinance.IAM.Domain.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, string imfCode, IEnumerable<string> roles);
}
