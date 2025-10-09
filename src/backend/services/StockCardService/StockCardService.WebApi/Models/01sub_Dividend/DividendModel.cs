namespace StockCardService.WebApi.Models._01sub_Dividend
{
    /// <summary>
    /// Модель дивиденда акции
    /// </summary>
    public class DividendModel
    {
        /// <summary>
        /// Id дивиденда акции
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Id акции
        /// </summary>
        public Guid ShareCardId { get; set; }

        /// <summary>
        /// дата див отсечки
        /// </summary>
        public DateTime CutOffDate { get; set; }

        /// <summary>
        /// период выплаты
        /// </summary>
        public required string Period { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        public required string Currency { get; set; }

        /// <summary>
        /// Размер дивидендов
        /// </summary>
        public decimal Value { get; set; }
    }
}
