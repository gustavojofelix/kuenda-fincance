using System;

namespace KuendaFinance.Operations.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
}
