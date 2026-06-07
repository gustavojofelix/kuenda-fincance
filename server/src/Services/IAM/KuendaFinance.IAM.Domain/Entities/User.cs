using System;
using KuendaFinance.Shared.Domain;

namespace KuendaFinance.IAM.Domain.Entities;

public class User : Entity
{
    public Guid TenantId { get; private set; }
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? Phone { get; private set; }
    public string? PhotoUrl { get; private set; }
    public bool IsActive { get; private set; }

    private User() { } // EF Core

    public User(Guid id, Guid tenantId, string email, string firstName, string lastName, string? phone = null, string? photoUrl = null) : base(id)
    {
        TenantId = tenantId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        PhotoUrl = photoUrl;
        IsActive = true;
    }

    public void UpdateProfile(string firstName, string lastName, string? phone, string? photoUrl)
    {
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        PhotoUrl = photoUrl;
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
