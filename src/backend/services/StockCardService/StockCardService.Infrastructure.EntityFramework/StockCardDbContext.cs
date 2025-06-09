using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.StockCardService.Models;

namespace StockMarketAssistant.StockCardService.Data
{     //*//
    public class StockCardDbContext : DbContext
    {
        public StockCardDbContext(DbContextOptions<StockCardDbContext> options) 
            : base(options) { }

        public DbSet<ShareCard> ShareCards { get; set; }
        public DbSet<FinancialReport> FinancialReports { get; set; }
        public DbSet<Multiplier> Multipliers { get; set; }
        public DbSet<Dividend> Dividends { get; set; }
        public DbSet<BondCard> BondCards { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CryptoCard> CryptoCards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ShareCard relationships
            modelBuilder.Entity<ShareCard>()
                .HasMany(s => s.FinancialReports)
                .WithOne(f => f.ShareCard)
                .HasForeignKey(f => f.ShareCardId);

            modelBuilder.Entity<ShareCard>()
                .HasMany(s => s.Multipliers)
                .WithOne(m => m.ShareCard)
                .HasForeignKey(m => m.ShareCardId);

            modelBuilder.Entity<ShareCard>()
                .HasMany(s => s.Dividends)
                .WithOne(d => d.ShareCard)
                .HasForeignKey(d => d.ShareCardId);

            // BondCard relationships
            modelBuilder.Entity<BondCard>()
                .HasMany(b => b.Coupons)
                .WithOne(c => c.Bond)
                .HasForeignKey(c => c.BondId);

            // Установка первичных ключей
            modelBuilder.Entity<ShareCard>().HasKey(s => s.Id);
            modelBuilder.Entity<FinancialReport>().HasKey(f => f.Id);
            modelBuilder.Entity<Multiplier>().HasKey(m => m.Id);
            modelBuilder.Entity<Dividend>().HasKey(d => d.Id);
            modelBuilder.Entity<BondCard>().HasKey(b => b.Id);
            modelBuilder.Entity<Coupon>().HasKey(c => c.Id);
            modelBuilder.Entity<CryptoCard>().HasKey(c => c.Id);

            // Настройка строковых свойств
            modelBuilder.Entity<ShareCard>()
                .Property(s => s.Ticker)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<BondCard>()
                .Property(b => b.Ticker)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<CryptoCard>()
                .Property(c => c.Ticker)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<FinancialReport>()
                .Property(f => f.Period)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Dividend>()
                .Property(d => d.Period)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Coupon>()
                .Property(c => c.Period)
                .IsRequired()
                .HasMaxLength(50);
        }
    }
}
