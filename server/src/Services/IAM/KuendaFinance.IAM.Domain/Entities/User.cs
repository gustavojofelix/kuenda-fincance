using System;
using KuendaFinance.Shared.Domain;

namespace KuendaFinance.IAM.Domain.Entities;

public class User : Entity
{
    public Guid TenantId { get; private set; }
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public bool IsActive { get; private set; }

    private User() { } // EF Core

    public User(Guid id, Guid tenantId, string email, string firstName, string lastName) : base(id)
    {
        TenantId = tenantId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
