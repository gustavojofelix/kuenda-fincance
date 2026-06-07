using System;
using System.Collections.Generic;
using KuendaFinance.IAM.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace KuendaFinance.IAM.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid TenantId { get; set; }
    
    // Auditing properties (manually implemented since we inherit from IdentityUser)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string? UpdatedBy { get; set; }

    public virtual ICollection<UserBranchRole> UserBranchRoles { get; set; } = new List<UserBranchRole>();
}
