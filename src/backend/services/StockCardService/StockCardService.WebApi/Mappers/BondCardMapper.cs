using StockCardService.WebApi.Models.BondCard;
using StockMarketAssistant.StockCardService.Application.DTOs._02_BondCard;
using StockMarketAssistant.StockCardService.Application.DTOs._02sub_Coupon;
using StockMarketAssistant.StockCardService.WebApi.Models._02sub_Coupon;

namespace StockMarketAssistant.StockCardService.WebApi.Mappers
{
    /// <summary>
    /// маппер для карточки облигации Dto/Model
    /// </summary>
    public static class BondCardMapper
    {
        public static BondCardDto ToDto(BondCardModel model)
        {
            if (model == null) return null;

            return new BondCardDto
            {
                Id = model.Id,
                Name = model.Name,
                Ticker = model.Ticker,
                Description = model.Description,
                Currency = model.Currency,
                CurrentPrice = model.CurrentPrice,
                FaceValue = model.FaceValue,
                Rating = model.Rating,
                MaturityPeriod = DateTime.SpecifyKind(model.MaturityPeriod, DateTimeKind.Utc),
                Coupons = model.Coupons?.Select(CouponMapper.ToDto).ToList() ?? new List<CouponDto>()
            };
        }

        public static CreatingBondCardDto ToDto(CreatingBondCardModel model)
        {
            if (model == null) return null;

            return new CreatingBondCardDto
            {
                Name = model.Name,
                Ticker = model.Ticker,
                Description = model.Description,
                Currency = model.Currency,
                FaceValue = model.FaceValue,
                Rating = model.Rating,
                MaturityPeriod = DateTime.SpecifyKind(model.MaturityPeriod, DateTimeKind.Utc)
            };
        }
        
        public static UpdatingBondCardDto ToDto(UpdatingBondCardModel model)
        {
            if (model == null) return null;

            return new UpdatingBondCardDto
            {
                Id = model.Id,
                Name = model.Name,
                Ticker = model.Ticker,
                Description = model.Description,
                Rating = model.Rating,
                MaturityPeriod = DateTime.SpecifyKind(model.MaturityPeriod, DateTimeKind.Utc)
            };
        }

        public static BondCardShortDto ToDto(BondCardShortModel model)
        {
            if (model == null) return null;

            return new BondCardShortDto
            {
                Id = model.Id,
                Name = model.Name,
                Ticker = model.Ticker,
                Description = model.Description,
                Currency = model.Currency,
                CurrentPrice = model.CurrentPrice,
                FaceValue = model.FaceValue,
                Rating = model.Rating,
                MaturityPeriod = model.MaturityPeriod.ToString()
            };
        }

        public static BondCardModel ToModel(BondCardDto dto)
        {
            if (dto == null) return null;

            return new BondCardModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Ticker = dto.Ticker,
                Description = dto.Description,
                Currency = dto.Currency,
                CurrentPrice = dto.CurrentPrice,
                FaceValue = dto.FaceValue,
                Rating = dto.Rating,
                MaturityPeriod = dto.MaturityPeriod,
                Coupons = dto.Coupons?.Select(CouponMapper.ToModel).ToList() ?? new List<CouponModel>()
            };
        }

        public static CreatingBondCardModel ToModel(CreatingBondCardDto dto)
        {
            if (dto == null) return null;

            return new CreatingBondCardModel
            {
                Name = dto.Name,
                Ticker = dto.Ticker,
                Description = dto.Description,
                Currency = dto.Currency,
                FaceValue = dto.FaceValue,
                Rating = dto.Rating,
                MaturityPeriod = dto.MaturityPeriod
            };
        }

        public static UpdatingBondCardModel ToModel(UpdatingBondCardDto dto)
        {
            if (dto == null) return null;

            return new UpdatingBondCardModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Ticker = dto.Ticker,
                Description = dto.Description,
                Rating = dto.Rating,
                MaturityPeriod = dto.MaturityPeriod
            };
        }

        public static BondCardShortModel ToModel(BondCardShortDto dto)
        {
            if (dto == null) return null;

            return new BondCardShortModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Ticker = dto.Ticker,
                Description = dto.Description,
                Currency = dto.Currency,
                CurrentPrice = dto.CurrentPrice,
                FaceValue = dto.FaceValue,
                Rating = dto.Rating,
                MaturityPeriod = dto.MaturityPeriod
            };
        }
    }
}
