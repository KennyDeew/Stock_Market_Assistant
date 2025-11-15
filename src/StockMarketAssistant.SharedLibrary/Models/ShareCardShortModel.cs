namespace StockMarketAssistant.SharedLibrary.Models
{
    /// <summary>
    /// Модель карточки акции
    /// </summary>
    public class ShareCardShortModel
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
        public static string Board => "TQBR";

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
    }
}
