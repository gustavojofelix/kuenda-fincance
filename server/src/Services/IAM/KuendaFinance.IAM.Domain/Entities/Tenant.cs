namespace KuendaFinance.IAM.Domain.Entities;

/// <summary>
/// Represents an IMF (Instituição Microfinanceira) tenant.
/// The Code maps to the subdomain (e.g., "imf-alpha" → imf-alpha.kuenda.com).
/// </summary>
public class Tenant
{
    public Guid Id { get; private set; }
    
    /// <summary>Unique subdomain code, e.g. "imf-alpha".</summary>
    public string Code { get; private set; }
    
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Tenant() { } // EF Core

    public Tenant(Guid id, string code, string name)
    {
        Id = id;
        Code = code.ToLowerInvariant().Trim();
        Name = name;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
