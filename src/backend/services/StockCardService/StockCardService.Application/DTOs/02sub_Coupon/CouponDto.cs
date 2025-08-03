namespace StockMarketAssistant.StockCardService.Application.DTOs._02sub_Coupon
{
    public class CouponDto
    {
        public Guid Id { get; set; }
        public Guid BondId { get; set; }

        public DateTime Period { get; set; }
        public string Currency { get; set; }
        public decimal Value { get; set; }
    }
}
