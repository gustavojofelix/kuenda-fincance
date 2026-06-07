using System;
using System.Collections.Generic;

namespace KuendaFinance.Operations.Application.DTOs;

public class ClientDto
{
    public Guid Id { get; set; }
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
    public string Status { get; set; } = string.Empty;
    public int LoanCycle { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string? UpdatedBy { get; set; }

    public List<GuaranteeDto> Guarantees { get; set; } = new();
}

public class GuaranteeDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
}
