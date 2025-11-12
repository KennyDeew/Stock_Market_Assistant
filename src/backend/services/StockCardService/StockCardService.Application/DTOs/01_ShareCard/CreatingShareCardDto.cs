namespace StockMarketAssistant.StockCardService.Application.DTOs._01_ShareCard
{
    /// <summary>
    /// Dto создаваемой карточки акции
    /// </summary>
    public class CreatingShareCardDto
    {
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
    }
}
