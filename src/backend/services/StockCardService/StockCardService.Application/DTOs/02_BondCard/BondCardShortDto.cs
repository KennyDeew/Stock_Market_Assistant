namespace StockMarketAssistant.StockCardService.Application.DTOs._02_BondCard
{
    /// <summary>
    /// Dto неполной карточки облигации
    /// </summary>
    public class BondCardShortDto
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
        /// Валюта
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// дата погашения облигации
        /// </summary>
        public string MaturityPeriod { get; set; }
    }
}
