namespace StockMarketAssistant.StockCardService.Application.DTOs._01sub_Dividend
{
    /// <summary>
    /// Dto измененного дивиденда акции
    /// </summary>
    public class UpdatingDividendDto
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
