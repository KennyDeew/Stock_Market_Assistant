namespace StockCardService.WebApi.Models._01sub_Dividend
{
    /// <summary>
    /// Модель измененного дивиденда акции
    /// </summary>
    public class UpdatingDividendModel
    {
        /// <summary>
        /// Id дивидендов
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// дата див отсечки
        /// </summary>
        public DateTime CutOffDate { get; set; }

        /// <summary>
        /// Размер дивидендов
        /// </summary>
        public decimal Value { get; set; }
    }
}
