using System;

namespace KuendaFinance.IAM.Domain.Common;

public interface ICurrentUserService
{
    string? UserId { get; }
}
