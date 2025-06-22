using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockCardService.WebApi.Models.ShareCard
{
    /// <summary>
    /// Модель карточки акции
    /// </summary>
    public class ShareCardModel
    {
        /// <summary>
        /// Id акции
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Тикер акции
        /// </summary>
        public string Ticker { get; set; }

        /// <summary>
        /// Наименование акции
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание акции
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Финансовые отчеты по акции
        /// </summary>
        public ICollection<FinancialReport> FinancialReports { get; set; }

        /// <summary>
        /// Мультипликаторы акции
        /// </summary>
        public ICollection<Multiplier> Multipliers { get; set; }

        /// <summary>
        /// Дивиденды акции
        /// </summary>
        public ICollection<Dividend> Dividends { get; set; }
    }
}
