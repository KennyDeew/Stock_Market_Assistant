using StockCardService.WebApi.Models.BondCard;
using StockMarketAssistant.StockCardService.Application.DTOs._02_BondCard;

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
                MaturityPeriod = DateTime.SpecifyKind(model.MaturityPeriod, DateTimeKind.Utc),
                Coupons = model.Coupons?.Select(CouponMapper.ToDto).ToList()
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
                MaturityPeriod = dto.MaturityPeriod,
                Coupons = dto.Coupons?.Select(CouponMapper.ToModel).ToList()
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
                MaturityPeriod = dto.MaturityPeriod
            };
        }
    }
}
