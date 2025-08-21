namespace StockMarketAssistant.StockCardService.WebApi.Models._02sub_Coupon
{
    /// <summary>
    /// Модель купона облигации
    /// </summary>
    public class CouponModel
    {
        /// <summary>
        /// Id купона
        /// </summary>
        public Guid Id { get; set; }

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
        public string Currency { get; set; }

        /// <summary>
        /// Размер купона
        /// </summary>
        public decimal Value { get; set; }
    }
}
