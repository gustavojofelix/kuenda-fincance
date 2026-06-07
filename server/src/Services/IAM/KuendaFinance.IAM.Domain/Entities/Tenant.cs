using KuendaFinance.Shared.Domain;

namespace KuendaFinance.IAM.Domain.Entities;

/// <summary>
/// Represents an IMF (Instituição Microfinanceira) tenant.
/// The Code maps to the subdomain (e.g., "imf-alpha" → imf-alpha.kuenda.com).
/// </summary>
public class Tenant : Entity
{
    /// <summary>Unique subdomain code, e.g. "imf-alpha".</summary>
    public string Code { get; private set; }
    
    public string Name { get; private set; }
    public string Subdomain { get; private set; }
    public string Nuit { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public string Address { get; private set; }
    public string PrimaryColor { get; private set; }
    public string SecondaryColor { get; private set; }
    public string SubscriptionPackage { get; private set; }
    public string? LogoUrl { get; private set; }
    public bool IsActive { get; private set; }

    private Tenant() { } // EF Core

    public Tenant(Guid id, string code, string name, string subdomain, string nuit, string email, string phone, string address, string primaryColor = "#6366f1", string secondaryColor = "#4f46e5", string subscriptionPackage = "Standard", string? logoUrl = null) : base(id)
    {
        Code = code.ToLowerInvariant().Trim();
        Name = name;
        Subdomain = subdomain.ToLowerInvariant().Trim();
        Nuit = nuit;
        Email = email;
        Phone = phone;
        Address = address;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        SubscriptionPackage = subscriptionPackage;
        LogoUrl = logoUrl;
        IsActive = true;
    }

    public void UpdateBranding(string name, string email, string phone, string address, string primaryColor, string secondaryColor, string? logoUrl)
    {
        Name = name;
        Email = email;
        Phone = phone;
        Address = address;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        LogoUrl = logoUrl;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
