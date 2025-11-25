using Microsoft.Extensions.Logging;
using StockCardService.Abstractions.Repositories;
using StockMarketAssistant.StockCardService.Domain.Entities;
using StockMarketAssistant.StockCardService.Infrastructure.EntityFramework;

namespace StockCardService.Infrastructure.EntityFramework
{
    //public static class DbInitializer
    //{
    //    public static void Initialize(StockCardDbContext context)
    //    {
    //        // Применяем миграции (если используются)
    //        context.Database.EnsureCreated();

    //        // ShareCards
    //        if (!context.ShareCards.Any())
    //        {
    //            context.ShareCards.AddRange(FakeDataFactory.ShareCards);
    //        }

    //        // BondCards
    //        if (!context.BondCards.Any())
    //        {
    //            context.BondCards.AddRange(FakeDataFactory.BondCards);
    //        }

    //        // Coupons
    //        if (!context.Coupons.Any())
    //        {
    //            context.Coupons.AddRange(FakeDataFactory.Coupons);
    //        }

    //        // Dividends
    //        if (!context.Dividends.Any())
    //        {
    //            context.Dividends.AddRange(FakeDataFactory.Dividends);
    //        }

    //        context.SaveChanges();
    //    }
    //}
    public class DbInitializer : IDbInitializer
    {
        private readonly StockCardDbContext _db;
        private readonly IMoexCardService _moex;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(
            StockCardDbContext db,
            IMoexCardService moex, 
            ILogger<DbInitializer> logger)
        {
            _db = db;
            _moex = moex;
            _logger = logger;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (!_db.ShareCards.Any())
            {
                _logger.LogInformation("START SHARES INITIALIZE");
                //Получаем список всех акций
                var shares = await _moex.GetShareCardsFromMoex(cancellationToken);
                //_logger.LogInformation($"count of shareCards - {shares.Count()}");
                //Доьавляем в БД
                _db.ShareCards.AddRange(shares);
                //Сохраняем
                await _db.SaveChangesAsync(cancellationToken);

                //Дивиденды
                const int batchSize = 500;
                var dividendsBuffer = new List<Dividend>(batchSize);

                foreach (var share in shares)
                {
                    //Получаем дивиденды с Мосбиржи
                    var dividends = await _moex.GetDividendsForShareCardFromMoex(share, cancellationToken);

                    foreach (var div in dividends)
                    {
                        dividendsBuffer.Add(div);

                        //Добавляем в БД и сохраняем пакетами
                        if (dividendsBuffer.Count >= batchSize)
                        {
                            _db.Dividends.AddRange(dividendsBuffer);
                            await _db.SaveChangesAsync(cancellationToken);

                            dividendsBuffer.Clear();
                            _db.ChangeTracker.Clear(); // очень важно!
                        }
                    }
                }

                // сохраняем хвост
                if (dividendsBuffer.Count > 0)
                {
                    _db.Dividends.AddRange(dividendsBuffer);
                    await _db.SaveChangesAsync(cancellationToken);
                    _db.ChangeTracker.Clear();
                }

                _logger.LogInformation("FINISHED SHARES INITIALIZATION");
            }

            //Облигации
            if (!_db.BondCards.Any())
            {
                //ОФЗ
                //_logger.LogError($"Start Bond Init");
                var bonds = await _moex.GetOFZBondCardsFromMoex(cancellationToken);
                _db.BondCards.AddRange(bonds);
                //await _db.SaveChangesAsync(cancellationToken);
                //Корпоративные облигации
                var corpBonds = await _moex.GetCorporateBondCardsFromMoex(cancellationToken);
                _db.BondCards.AddRange(corpBonds);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

