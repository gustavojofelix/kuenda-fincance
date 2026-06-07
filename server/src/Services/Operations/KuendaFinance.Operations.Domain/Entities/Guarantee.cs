using System;
using KuendaFinance.Shared.Domain;

namespace KuendaFinance.Operations.Domain.Entities;

public class Guarantee : Entity
{
    public Guarantee(Guid id) : base(id) { }

    public Guarantee() : base() { }

    public Guid ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
}
