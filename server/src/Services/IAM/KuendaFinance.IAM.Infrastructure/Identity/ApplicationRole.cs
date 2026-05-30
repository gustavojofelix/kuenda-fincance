using Microsoft.AspNetCore.Identity;

namespace KuendaFinance.IAM.Infrastructure.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public string Description { get; set; } = string.Empty;
}
