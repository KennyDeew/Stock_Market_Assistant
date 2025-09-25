namespace StockMarketAssistant.StockCardService.Application.DTOs._02sub_Coupon
{
    /// <summary>
    /// Dto купона облигации
    /// </summary>
    public class CouponDto
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
        public required string Currency { get; set; }

        /// <summary>
        /// Размер купона
        /// </summary>
        public decimal Value { get; set; }
    }
}
