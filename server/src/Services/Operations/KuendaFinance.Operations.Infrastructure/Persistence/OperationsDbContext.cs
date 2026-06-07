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
    public DbSet<ClientStatusHistory> ClientStatusHistories { get; set; } = null!;
    public DbSet<Loan> Loans { get; set; } = null!;
    public DbSet<Installment> Installments { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<CreditSettings> CreditSettings { get; set; } = null!;

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

            // Global query filter for Multi-Tenancy
            b.HasQueryFilter(c => c.TenantId == _currentUserService.TenantId);

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

        modelBuilder.Entity<ClientStatusHistory>(b =>
        {
            b.ToTable("ClientStatusHistories");
            b.HasKey(h => h.Id);
        });

        modelBuilder.Entity<Loan>(b =>
        {
            b.ToTable("Loans");
            b.HasKey(l => l.Id);
            
            // Multi-Tenancy Filter
            b.HasQueryFilter(l => l.TenantId == _currentUserService.TenantId);

            b.HasMany(l => l.Installments)
             .WithOne()
             .HasForeignKey(i => i.LoanId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(l => l.Payments)
             .WithOne()
             .HasForeignKey(p => p.LoanId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Installment>(b =>
        {
            b.ToTable("Installments");
            b.HasKey(i => i.Id);
        });

        modelBuilder.Entity<Payment>(b =>
        {
            b.ToTable("Payments");
            b.HasKey(p => p.Id);
        });

        modelBuilder.Entity<Transaction>(b =>
        {
            b.ToTable("Transactions");
            b.HasKey(t => t.Id);
            b.HasQueryFilter(t => t.TenantId == _currentUserService.TenantId);
            b.HasIndex(t => t.TransactionDate);
            b.HasIndex(t => t.Category);
        });

        modelBuilder.Entity<CreditSettings>(b =>
        {
            b.ToTable("CreditSettings");
            b.HasKey(s => s.Id);
            b.HasQueryFilter(s => s.TenantId == _currentUserService.TenantId);
            b.HasIndex(s => s.TenantId).IsUnique();
        });
    }
}
