namespace StockMarketAssistant.StockCardService.Application.DTOs._02sub_Coupon
{
    /// <summary>
    /// Dto измененного купона облигации
    /// </summary>
    public class UpdatingCouponDto
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
        public DateTime Period { get; set; }

        /// <summary>
        /// Размер купона
        /// </summary>
        public decimal Value { get; set; }
    }
}
