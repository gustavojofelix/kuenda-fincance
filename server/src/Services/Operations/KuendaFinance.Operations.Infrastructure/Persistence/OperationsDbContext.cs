using System;
using System.Threading;
using System.Threading.Tasks;
using KuendaFinance.Operations.Application.Interfaces;
using KuendaFinance.Operations.Domain.Entities;
using KuendaFinance.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace KuendaFinance.Operations.Infrastructure.Persistence;

public class OperationsDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public OperationsDbContext(
        DbContextOptions<OperationsDbContext> options,
        ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Guarantee> Guarantees { get; set; } = null!;

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
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Client>(b =>
        {
            b.ToTable("Clients");
            b.HasKey(c => c.Id);

            // Composite unique index for TenantId + BI to enforce uniqueness within a tenant
            b.HasIndex(c => new { c.TenantId, c.BI }).IsUnique();

            b.HasMany(c => c.Guarantees)
             .WithOne()
             .HasForeignKey(g => g.ClientId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Guarantee>(b =>
        {
            b.ToTable("Guarantees");
            b.HasKey(g => g.Id);
        });
    }
}
