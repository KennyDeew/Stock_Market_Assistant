using StockMarketAssistant.StockCardService.Infrastructure.EntityFramework;

namespace StockCardService.Infrastructure.EntityFramework
{
    public static class DbInitializer
    {
        public static void Initialize(StockCardDbContext context)
        {
            // Применяем миграции (если используются)
            context.Database.EnsureCreated();

            // ShareCards
            if (!context.ShareCards.Any())
            {
                context.ShareCards.AddRange(FakeDataFactory.ShareCards);
            }

            // BondCards
            if (!context.BondCards.Any())
            {
                context.BondCards.AddRange(FakeDataFactory.BondCards);
            }

            // Coupons
            if (!context.Coupons.Any())
            {
                context.Coupons.AddRange(FakeDataFactory.Coupons);
            }

            // Dividends
            if (!context.Dividends.Any())
            {
                context.Dividends.AddRange(FakeDataFactory.Dividends);
            }

            context.SaveChanges();
        }
    }
}

