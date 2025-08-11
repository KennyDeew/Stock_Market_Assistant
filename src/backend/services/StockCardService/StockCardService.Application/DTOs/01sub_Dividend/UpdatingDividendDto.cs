namespace StockMarketAssistant.StockCardService.Application.DTOs._01sub_Dividend
{
    /// <summary>
    /// Dto измененного дивиденда акции
    /// </summary>
    public class UpdatingDividendDto
    {
        /// <summary>
        /// дата див отсечки
        /// </summary>
        public DateTime CuttOffDate { get; set; }

        /// <summary>
        /// Размер дивидендов
        /// </summary>
        public decimal Value { get; set; }
    }
}
