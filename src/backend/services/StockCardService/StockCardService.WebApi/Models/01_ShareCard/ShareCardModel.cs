using StockCardService.WebApi.Models._01sub_Dividend;
using StockCardService.WebApi.Models._01sub_FinancialReport;
using StockCardService.WebApi.Models._01sub_Multiplier;
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
        public required string Ticker { get; set; }

        /// <summary>
        /// Наименование акции
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Режим торгов (АКЦИИ - TQBR, КорпОбл - TQCB, ОФЗ - TQOB)
        /// </summary>
        public string Board => "TQBR";

        /// <summary>
        /// Описание акции
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        public required string Currency { get; set; }

        /// <summary>
        /// Текущая цена
        /// </summary>
        public decimal CurrentPrice { get; set; }

        /// <summary>
        /// Финансовые отчеты по акции
        /// </summary>
        public List<FinancialReportModel> FinancialReports { get; set; } = new List<FinancialReportModel>();

        /// <summary>
        /// Мультипликаторы акции
        /// </summary>
        public List<MultiplierModel> Multipliers { get; set; } = new List<MultiplierModel>();

        /// <summary>
        /// Дивиденды акции
        /// </summary>
        public List<DividendModel> Dividends { get; set; } = new List<DividendModel>();
    }
}
