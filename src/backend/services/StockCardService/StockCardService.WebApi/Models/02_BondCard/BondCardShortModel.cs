using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockCardService.WebApi.Models.BondCard
{
    /// <summary>
    /// Модель карточки облигации
    /// </summary>
    public class BondCardShortModel
    {
        /// <summary>
        /// Id облигации
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Тикер облигации
        /// </summary>
        public string Ticker { get; set; }

        /// <summary>
        /// Наименование облигации
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание облигации
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// дата погашения облигации
        /// </summary>
        public string MaturityPeriod { get; set; }

    }
}
