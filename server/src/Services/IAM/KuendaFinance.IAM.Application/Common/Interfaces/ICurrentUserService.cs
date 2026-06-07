using System;

namespace KuendaFinance.IAM.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
}
