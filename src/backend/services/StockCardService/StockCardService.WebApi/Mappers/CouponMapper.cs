using StockMarketAssistant.StockCardService.Application.DTOs._02sub_Coupon;
using StockMarketAssistant.StockCardService.WebApi.Models._02sub_Coupon;

namespace StockMarketAssistant.StockCardService.WebApi.Mappers
{
    /// <summary>
    /// маппер для купонов облигаций Dto/Model
    /// </summary>
    public class CouponMapper
    {
        public static CouponDto ToDto(CouponModel model)
        {
            if (model == null) return null;

            return new CouponDto
            {
                Id = model.Id,
                BondId = model.BondId,
                Period = model.Period,
                Currency = model.Currency,
                Value = model.Value
                
            };
        }

        public static CouponModel ToModel(CouponDto dto)
        {
            if (dto == null) return null;

            return new CouponModel
            {
                Id = dto.Id,
                BondId = dto.BondId,
                Period = dto.Period,
                Currency = dto.Currency,
                Value = dto.Value
            };
        }
    }
}
