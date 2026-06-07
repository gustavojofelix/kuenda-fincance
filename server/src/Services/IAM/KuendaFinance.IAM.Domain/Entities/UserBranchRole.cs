using System;
using KuendaFinance.Shared.Domain;

namespace KuendaFinance.IAM.Domain.Entities;

public class UserBranchRole : Entity
{
    public Guid UserId { get; private set; }
    public Guid BranchId { get; private set; }
    public string Role { get; private set; } // "Administrator", "Manager", "Agent", etc.

    private UserBranchRole() { } // EF Core

    public UserBranchRole(Guid id, Guid userId, Guid branchId, string role) : base(id)
    {
        UserId = userId;
        BranchId = branchId;
        Role = role;
    }

    public void UpdateRole(string role)
    {
        Role = role;
    }
}
