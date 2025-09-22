namespace StockMarketAssistant.StockCardService.WebApi.Models._02sub_Coupon
{
    /// <summary>
    /// Модель создаваемого купона облигации
    /// </summary>
    public class CreatingCouponModel
    {
        /// <summary>
        /// Id облигации
        /// </summary>
        public Guid BondId { get; set; }

        /// <summary>
        /// дата фиксирования владельца облигацией для выплаты купона
        /// </summary>
        public DateTime CutOffDate { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        public required string Currency { get; set; }

        /// <summary>
        /// Размер купона
        /// </summary>
        public decimal Value { get; set; }
    }
}
