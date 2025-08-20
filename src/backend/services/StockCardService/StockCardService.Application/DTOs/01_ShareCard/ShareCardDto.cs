using StockMarketAssistant.StockCardService.Application.DTOs._01sub_Dividend;
using StockMarketAssistant.StockCardService.Application.DTOs._01sub_FinancialReport;
using StockMarketAssistant.StockCardService.Application.DTOs._01sub_Multiplier;

namespace StockMarketAssistant.StockCardService.Application.DTOs._01_ShareCard
{
    /// <summary>
    /// Dto карточки акции
    /// </summary>
    public class ShareCardDto
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
        /// Валюта
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Текущая цена
        /// </summary>
        public decimal CurrentPrice { get; set; }

        /// <summary>
        /// Финансовые отчеты по акции
        /// </summary>
        public List<FinancialReportDto>? FinancialReports { get; set; }

        /// <summary>
        /// Мультипликаторы акции
        /// </summary>
        public List<MultiplierDto>? Multipliers { get; set; }

        /// <summary>
        /// Дивиденды акции
        /// </summary>
        public List<DividendDto>? Dividends { get; set; }
    }
}
