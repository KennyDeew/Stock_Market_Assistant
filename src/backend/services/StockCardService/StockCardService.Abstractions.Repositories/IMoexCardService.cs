using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockCardService.Abstractions.Repositories
{
    /// <summary>
    /// Интерфейс для создания карточек акций, облигаций из API Мосбиржи
    /// </summary>
    public interface IMoexCardService
    {
        /// <summary>
        /// Получить акции с Мосбиржи
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<ShareCard>> GetShareCardsFromMoex(CancellationToken cancellationToken);

        /// <summary>
        /// Получить дивиденды акции с Мосбиржи
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<Dividend>> GetDividendsForShareCardFromMoex(ShareCard shareCard, CancellationToken cancellationToken);

        /// <summary>
        /// Получить корпоративные облигации с Мосбиржи
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<BondCard>> GetCorporateBondCardsFromMoex(CancellationToken cancellationToken);

        /// <summary>
        /// Получить ОФЗ с Мосбиржи
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<BondCard>> GetOFZBondCardsFromMoex(CancellationToken cancellationToken);
    }
}
