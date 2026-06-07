using System;
using KuendaFinance.Shared.Domain;

namespace KuendaFinance.Operations.Domain.Entities;

public class ClientStatusHistory : Entity
{
    public ClientStatusHistory(Guid id) : base(id) { }

    public ClientStatusHistory() : base() { }

    public Guid ClientId { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string ChangedBy { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
