namespace StockMarketAssistant.StockCardService.Application.DTOs._02_BondCard
{
    /// <summary>
    /// Dto создаваемой карточки облигации
    /// </summary>
    public class CreatingBondCardDto
    {
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
        public DateTime MaturityPeriod { get; set; }

        /// <summary>
        /// Рейтинг
        /// </summary>
        public string Rating { get; set; }

        /// <summary>
        /// Номинальная стоимость
        /// </summary>
        public decimal FaceValue { get; set; }
    }
}
