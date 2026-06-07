using System;
using KuendaFinance.Shared.Domain;

namespace KuendaFinance.IAM.Domain.Entities;

public class Branch : Entity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string Cellphone { get; private set; }
    public string Email { get; private set; }
    public string City { get; private set; }
    public string Address { get; private set; }
    public string Manager { get; private set; }
    public string Status { get; private set; } // "Active" or "Inactive"

    private Branch() { } // EF Core

    public Branch(Guid id, Guid tenantId, string name, string cellphone, string email, string city, string address, string manager, string status = "Active") : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Cellphone = cellphone;
        Email = email;
        City = city;
        Address = address;
        Manager = manager;
        Status = status;
    }

    public void UpdateDetails(string name, string cellphone, string email, string city, string address, string manager, string status)
    {
        Name = name;
        Cellphone = cellphone;
        Email = email;
        City = city;
        Address = address;
        Manager = manager;
        Status = status;
    }
}
