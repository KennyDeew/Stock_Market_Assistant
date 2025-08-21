using StockCardService.WebApi.Models.CryptoCard;
using StockMarketAssistant.StockCardService.Application.DTOs._03_CryptoCard;

namespace StockMarketAssistant.StockCardService.WebApi.Mappers
{
    /// <summary>
    /// маппер для карточки криптовалюты Dto/Model
    /// </summary>
    public static class CryptoCardMapper
    {
        public static CryptoCardDto ToDto(CryptoCardModel model)
        {
            if (model == null) return null;

            return new CryptoCardDto
            {
                Id = model.Id,
                Name = model.Name,
                Ticker = model.Ticker,
                CurrentPrice = model.CurrentPrice,
                Description = model.Description
            };
        }

        public static CreatingCryptoCardDto ToDto(CreatingCryptoCardModel model)
        {
            if (model == null) return null;

            return new CreatingCryptoCardDto
            {
                Name = model.Name,
                Ticker = model.Ticker,
                Description = model.Description
            };
        }

        public static UpdatingCryptoCardDto ToDto(UpdatingCryptoCardModel model)
        {
            if (model == null) return null;

            return new UpdatingCryptoCardDto
            {
                Id = model.Id,
                Name = model.Name,
                Ticker = model.Ticker,
                CurrentPrice = model.CurrentPrice,
                Description = model.Description
            };
        }

        public static CryptoCardModel ToModel(CryptoCardDto dto)
        {
            if (dto == null) return null;

            return new CryptoCardModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Ticker = dto.Ticker,
                CurrentPrice = dto.CurrentPrice,
                Description = dto.Description
            };
        }

        public static CreatingCryptoCardModel ToModel(CreatingCryptoCardDto dto)
        {
            if (dto == null) return null;

            return new CreatingCryptoCardModel
            {
                Name = dto.Name,
                Ticker = dto.Ticker,
                Description = dto.Description
            };
        }

        public static UpdatingCryptoCardModel ToModel(UpdatingCryptoCardDto dto)
        {
            if (dto == null) return null;

            return new UpdatingCryptoCardModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Ticker = dto.Ticker,
                CurrentPrice = dto.CurrentPrice,
                Description = dto.Description
            };
        }
    }
}
