namespace StockMarketAssistant.StockCardService.Application.DTOs._02_BondCard
{
    /// <summary>
    /// Dto измененной карточки облигации
    /// </summary>
    public class UpdatingBondCardDto
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
        public DateTime MaturityPeriod { get; set; }

        /// <summary>
        /// Рейтинг
        /// </summary>
        public string Rating { get; set; }
    }
}
