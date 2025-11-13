namespace StockCardService.WebApi.Models.BondCard
{
    /// <summary>
    /// Модель создаваемой карточки облигации
    /// </summary>
    public class CreatingBondCardModel
    {
        /// <summary>
        /// Тикер облигации
        /// </summary>
        public required string Ticker { get; set; }

        /// <summary>
        /// Наименование облигации
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Режим торгов (АКЦИИ - TQBR, КорпОбл - TQCB, ОФЗ - TQOB)
        /// </summary>
        public required string Board { get; set; }

        /// <summary>
        /// Описание облигации
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// дата погашения облигации
        /// </summary>
        public DateTime MaturityPeriod { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        public required string Currency { get; set; }

        /// <summary>
        /// Рейтинг
        /// </summary>
        public required string Rating { get; set; }

        /// <summary>
        /// Номинальная стоимость
        /// </summary>
        public decimal FaceValue { get; set; }
    }
}
