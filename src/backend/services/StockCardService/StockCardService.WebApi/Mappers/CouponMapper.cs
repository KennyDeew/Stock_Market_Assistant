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
                Currency = model.Currency,
                Value = model.Value
            };
        }

        public static CreatingCouponDto ToDto(CreatingCouponModel model)
        {
            if (model == null) return null;

            return new CreatingCouponDto
            {
                BondId = model.BondId,
                CutOffDate = model.CutOffDate,
                Currency = model.Currency,
                Value = model.Value
            };
        }

        public static UpdatingCouponDto ToDto(UpdatingCouponModel model)
        {
            if (model == null) return null;

            return new UpdatingCouponDto
            {
                CutOffDate = model.CutOffDate,
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
                Currency = dto.Currency,
                Value = dto.Value
            };
        }

        public static CreatingCouponModel ToDto(CreatingCouponDto dto)
        {
            if (dto == null) return null;

            return new CreatingCouponModel
            {
                BondId = dto.BondId,
                CutOffDate = dto.CutOffDate,
                Currency = dto.Currency,
                Value = dto.Value
            };
        }

        public static UpdatingCouponModel ToDto(UpdatingCouponDto dto)
        {
            if (dto == null) return null;

            return new UpdatingCouponModel
            {
                CutOffDate = dto.CutOffDate,
                Value = dto.Value
            };
        }
    }
}
