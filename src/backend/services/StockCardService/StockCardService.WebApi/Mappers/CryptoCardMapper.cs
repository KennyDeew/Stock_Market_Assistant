using StockCardService.WebApi.Models.CryptoCard;
using StockMarketAssistant.StockCardService.Application.DTOs._03_CryptoCard;

namespace StockMarketAssistant.StockCardService.WebApi.Mappers
{
    /// <summary>
    /// Маппер для карточки криптовалюты model/dto.
    /// </summary>
    public static class CryptoCardMapper
    {
        /// <summary>
        /// Конвертирует модель карточки криптовалюты в DTO.
        /// </summary>
        /// <param name="model">Исходная модель карточки криптовалюты.</param>
        /// <returns>DTO карточки криптовалюты.</returns>
        public static CryptoCardDto ToDto(CryptoCardModel model)
        {
            return new CryptoCardDto
            {
                Id = model.Id,
                Name = model.Name,
                Ticker = model.Ticker,
                CurrentPrice = model.CurrentPrice,
                Description = model.Description
            };
        }

        /// <summary>
        /// Конвертирует модель создаваемой карточки криптовалюты в DTO.
        /// </summary>
        /// <param name="model">Исходная модель создаваемой карточки криптовалюты.</param>
        /// <returns>DTO создаваемой карточки криптовалюты.</returns>
        public static CreatingCryptoCardDto ToDto(CreatingCryptoCardModel model)
        {
            return new CreatingCryptoCardDto
            {
                Name = model.Name,
                Ticker = model.Ticker,
                Description = model.Description
            };
        }

        /// <summary>
        /// Конвертирует модель изменяемой карточки криптовалюты в DTO.
        /// </summary>
        /// <param name="model">Исходная модель изменяемой карточки криптовалюты.</param>
        /// <returns>DTO изменяемой карточки криптовалюты.</returns>
        public static UpdatingCryptoCardDto ToDto(UpdatingCryptoCardModel model)
        {
            return new UpdatingCryptoCardDto
            {
                Id = model.Id,
                Name = model.Name,
                Ticker = model.Ticker,
                CurrentPrice = model.CurrentPrice,
                Description = model.Description
            };
        }

        /// <summary>
        /// Конвертирует DTO карточки криптовалюты в модель.
        /// </summary>
        /// <param name="dto">Исходный DTO карточки криптовалюты.</param>
        /// <returns>Модель карточки криптовалюты.</returns>
        public static CryptoCardModel ToModel(CryptoCardDto dto)
        {
            return new CryptoCardModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Ticker = dto.Ticker,
                CurrentPrice = dto.CurrentPrice,
                Description = dto.Description
            };
        }

        /// <summary>
        /// Конвертирует DTO создаваемой карточки криптовалюты в модель.
        /// </summary>
        /// <param name="dto">Исходный DTO создаваемой карточки криптовалюты.</param>
        /// <returns>Модель создаваемой карточки криптовалюты.</returns>
        public static CreatingCryptoCardModel ToModel(CreatingCryptoCardDto dto)
        {
            return new CreatingCryptoCardModel
            {
                Name = dto.Name,
                Ticker = dto.Ticker,
                Description = dto.Description
            };
        }

        /// <summary>
        /// Конвертирует DTO изменяемой карточки криптовалюты в модель.
        /// </summary>
        /// <param name="dto">Исходный DTO изменяемой карточки криптовалюты.</param>
        /// <returns>Модель изменяемой карточки криптовалюты.</returns>
        public static UpdatingCryptoCardModel ToModel(UpdatingCryptoCardDto dto)
        {
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