using CashewNuts.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashewNuts.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<CashewType> CashewTypes => Set<CashewType>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        // ── Relationships ──────────────────────────────────────────────────
        m.Entity<Purchase>().HasMany(p => p.Items).WithOne(i => i.Purchase).HasForeignKey(i => i.PurchaseId);
        m.Entity<Sale>().HasMany(s => s.Items).WithOne(i => i.Sale).HasForeignKey(i => i.SaleId);

        // ✅ WithMany() — no nav property needed on CashewType
        m.Entity<PurchaseItem>().HasOne(i => i.CashewType).WithMany().HasForeignKey(i => i.CashewTypeId);
        m.Entity<SaleItem>().HasOne(i => i.CashewType).WithMany().HasForeignKey(i => i.CashewTypeId);

        // ── CashewType → User ──────────────────────────────────────────────
        m.Entity<CashewType>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .IsRequired(false)              // ← nullable
            .OnDelete(DeleteBehavior.Restrict);

        // ── Decimal precision ──────────────────────────────────────────────
        foreach (var e in new[] { "QtyKg", "PricePerKg", "Total" })
        {
            m.Entity<PurchaseItem>().Property(e).HasColumnType("decimal(18,3)");
            m.Entity<SaleItem>().Property(e).HasColumnType("decimal(18,3)");
        }
        m.Entity<Purchase>().Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");
        m.Entity<Sale>().Property(s => s.TotalAmount).HasColumnType("decimal(18,2)");
        m.Entity<CashewType>().Property(t => t.DefaultPrice).HasColumnType("decimal(18,2)");

        // ── Indexes ────────────────────────────────────────────────────────
        m.Entity<Purchase>().HasIndex(p => p.PurchaseDate).HasDatabaseName("IX_Purchases_PurchaseDate");
        m.Entity<Purchase>().HasIndex(p => p.UserId).HasDatabaseName("IX_Purchases_UserId");
        m.Entity<Sale>().HasIndex(s => s.SaleDate).HasDatabaseName("IX_Sales_SaleDate");
        m.Entity<Sale>().HasIndex(s => s.UserId).HasDatabaseName("IX_Sales_UserId");
        m.Entity<PurchaseItem>().HasIndex(i => i.PurchaseId).HasDatabaseName("IX_PurchaseItems_PurchaseId");
        m.Entity<SaleItem>().HasIndex(i => i.SaleId).HasDatabaseName("IX_SaleItems_SaleId");
        m.Entity<CashewType>().HasIndex(t => t.UserId).HasDatabaseName("IX_CashewTypes_UserId");
    }
}