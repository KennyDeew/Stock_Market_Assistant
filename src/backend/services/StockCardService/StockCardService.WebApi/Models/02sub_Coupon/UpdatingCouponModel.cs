namespace StockMarketAssistant.StockCardService.WebApi.Models._02sub_Coupon
{
    /// <summary>
    /// Модель измененного купона облигации
    /// </summary>
    public class UpdatingCouponModel
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
        /// Размер купона
        /// </summary>
        public decimal Value { get; set; }
    }
}
