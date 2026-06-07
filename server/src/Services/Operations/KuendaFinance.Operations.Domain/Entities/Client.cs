using System;
using System.Collections.Generic;
using KuendaFinance.Shared.Domain;

namespace KuendaFinance.Operations.Domain.Entities;

public class Client : Entity
{
    public Client(Guid id) : base(id)
    {
        Guarantees = new List<Guarantee>();
    }

    public Client() : base()
    {
        Guarantees = new List<Guarantee>();
    }

    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BI { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string MaritalStatus { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Business { get; set; } = string.Empty;
    public string BusinessYears { get; set; } = string.Empty;
    public string Income { get; set; } = string.Empty;
    public string EmergencyName { get; set; } = string.Empty;
    public string EmergencyRelation { get; set; } = string.Empty;
    public string EmergencyPhone { get; set; } = string.Empty;
    public string Status { get; set; } = "Evaluation";
    public int LoanCycle { get; set; } = 0;

    public ICollection<Guarantee> Guarantees { get; set; }
}
