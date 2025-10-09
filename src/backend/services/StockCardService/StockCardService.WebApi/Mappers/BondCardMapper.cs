using StockCardService.WebApi.Models.BondCard;
using StockMarketAssistant.StockCardService.Application.DTOs._02_BondCard;

namespace StockMarketAssistant.StockCardService.WebApi.Mappers
{
    /// <summary>
    /// Маппер для карточки облигации model/dto.
    /// </summary>
    public static class BondCardMapper
    {
        /// <summary>
        /// Конвертирует модель карточки облигации в DTO.
        /// </summary>
        /// <param name="model">Исходная модель карточки облигации.</param>
        /// <returns>DTO карточки облигации.</returns>
        public static BondCardDto ToDto(BondCardModel model)
        {
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
                Coupons = model.Coupons.Select(CouponMapper.ToDto).ToList()
            };
        }

        /// <summary>
        /// Конвертирует модель создаваемой карточки облигации в DTO.
        /// </summary>
        /// <param name="model">Исходная модель создаваемой карточки облигации.</param>
        /// <returns>DTO создаваемой карточки облигации.</returns>
        public static CreatingBondCardDto ToDto(CreatingBondCardModel model)
        {
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

        /// <summary>
        /// Конвертирует модель изменяемой карточки облигации в DTO.
        /// </summary>
        /// <param name="model">Исходная модель изменяемой карточки облигации.</param>
        /// <returns>DTO изменяемой карточки облигации.</returns>
        public static UpdatingBondCardDto ToDto(UpdatingBondCardModel model)
        {
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

        /// <summary>
        /// Конвертирует сокращённую модель карточки облигации в DTO.
        /// </summary>
        /// <param name="model">Исходная сокращённая модель карточки облигации.</param>
        /// <returns>DTO сокращённой карточки облигации.</returns>
        public static BondCardShortDto ToDto(BondCardShortModel model)
        {
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

        /// <summary>
        /// Конвертирует DTO карточки облигации в модель.
        /// </summary>
        /// <param name="dto">Исходный DTO карточки облигации.</param>
        /// <returns>Модель карточки облигации.</returns>
        public static BondCardModel ToModel(BondCardDto dto)
        {
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
                Coupons = dto.Coupons.Select(CouponMapper.ToModel).ToList()
            };
        }

        /// <summary>
        /// Конвертирует DTO создаваемой карточки облигации в модель.
        /// </summary>
        /// <param name="dto">Исходный DTO создаваемой карточки облигации.</param>
        /// <returns>Модель создаваемой карточки облигации.</returns>
        public static CreatingBondCardModel ToModel(CreatingBondCardDto dto)
        {
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

        /// <summary>
        /// Конвертирует DTO изменяемой карточки облигации в модель.
        /// </summary>
        /// <param name="dto">Исходный DTO изменяемой карточки облигации.</param>
        /// <returns>Модель изменяемой карточки облигации.</returns>
        public static UpdatingBondCardModel ToModel(UpdatingBondCardDto dto)
        {
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

        /// <summary>
        /// Конвертирует сокращённый DTO карточки облигации в модель.
        /// </summary>
        /// <param name="dto">Исходный сокращённый DTO карточки облигации.</param>
        /// <returns>Сокращённая модель карточки облигации.</returns>
        public static BondCardShortModel ToModel(BondCardShortDto dto)
        {
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