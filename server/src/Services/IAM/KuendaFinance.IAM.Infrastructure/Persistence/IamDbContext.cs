using KuendaFinance.IAM.Application.Common.Interfaces;
using KuendaFinance.IAM.Domain.Entities;
using KuendaFinance.IAM.Infrastructure.Identity;
using KuendaFinance.Shared.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KuendaFinance.IAM.Infrastructure.Persistence;

public class IamDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly ICurrentUserService _currentUserService;

    public IamDbContext(
        DbContextOptions<IamDbContext> options,
        ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Branch> Branches { get; set; } = null!;
    public DbSet<UserBranchRole> UserBranchRoles { get; set; } = null!;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries();
        var userId = _currentUserService.UserId ?? "system";
        
        foreach (var entry in entries)
        {
            if (entry.Entity is Entity entity)
            {
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                    entity.CreatedBy = userId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.LastUpdated = DateTime.UtcNow;
                    entity.UpdatedBy = userId;
                }
            }
            else if (entry.Entity is ApplicationUser user)
            {
                if (entry.State == EntityState.Added)
                {
                    user.CreatedAt = DateTime.UtcNow;
                    user.CreatedBy = userId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    user.LastUpdated = DateTime.UtcNow;
                    user.UpdatedBy = userId;
                }
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize the ASP.NET Identity model and override the defaults if needed.
        
        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("Users");
            b.HasOne<Tenant>()
             .WithMany()
             .HasForeignKey(u => u.TenantId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ApplicationRole>(b =>
        {
            b.ToTable("Roles");
        });

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>(b =>
        {
            b.ToTable("UserRoles");
        });

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>(b =>
        {
            b.ToTable("UserClaims");
        });

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>(b =>
        {
            b.ToTable("UserLogins");
        });

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>(b =>
        {
            b.ToTable("RoleClaims");
        });

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>(b =>
        {
            b.ToTable("UserTokens");
        });

        // Multitenancy Entity Configurations
        builder.Entity<Tenant>(b =>
        {
            b.ToTable("Tenants");
            b.HasKey(t => t.Id);
            b.HasIndex(t => t.Code).IsUnique();
            b.HasIndex(t => t.Subdomain).IsUnique();
            b.Property(t => t.Code).IsRequired().HasMaxLength(50);
            b.Property(t => t.Subdomain).IsRequired().HasMaxLength(100);
            b.Property(t => t.Name).IsRequired().HasMaxLength(150);
            b.Property(t => t.Nuit).IsRequired().HasMaxLength(50);
        });

        builder.Entity<Branch>(b =>
        {
            b.ToTable("Branches");
            b.HasKey(br => br.Id);
            b.HasOne<Tenant>()
             .WithMany()
             .HasForeignKey(br => br.TenantId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserBranchRole>(b =>
        {
            b.ToTable("UserBranchRoles");
            b.HasKey(ubr => ubr.Id);
            
            // Composite index to enforce unique branch membership per user
            b.HasIndex(ubr => new { ubr.UserId, ubr.BranchId }).IsUnique();

            b.HasOne<ApplicationUser>()
             .WithMany(u => u.UserBranchRoles)
             .HasForeignKey(ubr => ubr.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<Branch>()
             .WithMany()
             .HasForeignKey(ubr => ubr.BranchId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
