using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.StockCardService.Domain.Entities;


namespace StockMarketAssistant.StockCardService.Infrastructure.EntityFramework

{     //*//
    public class StockCardDbContext : DbContext
    {
        public StockCardDbContext(DbContextOptions<StockCardDbContext> options) 
            : base(options) { }

        public DbSet<ShareCard> ShareCards { get; set; }
        public DbSet<Multiplier> Multipliers { get; set; }
        public DbSet<Dividend> Dividends { get; set; }
        public DbSet<BondCard> BondCards { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CryptoCard> CryptoCards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ShareCard relationships
            modelBuilder.Entity<ShareCard>()
                .HasMany(s => s.Multipliers)
                .WithOne(m => m.ShareCard)
                .HasForeignKey(m => m.ParentId);

            modelBuilder.Entity<ShareCard>()
                .HasMany(s => s.Dividends)
                .WithOne(d => d.ShareCard)
                .HasForeignKey(d => d.ParentId);

            // BondCard relationships
            modelBuilder.Entity<BondCard>()
                .HasMany(b => b.Coupons)
                .WithOne(c => c.Bond)
                .HasForeignKey(c => c.ParentId);

            // Установка первичных ключей
            modelBuilder.Entity<ShareCard>().HasKey(s => s.Id);
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

            modelBuilder.Entity<Dividend>()
                .Property(d => d.Period)
                .IsRequired()
                .HasMaxLength(50);

        }
    }
}

