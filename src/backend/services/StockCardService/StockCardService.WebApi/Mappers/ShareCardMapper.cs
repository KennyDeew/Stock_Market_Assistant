using StockCardService.WebApi.Models.ShareCard;
using StockMarketAssistant.StockCardService.Application.DTOs._01_ShareCard;

namespace StockMarketAssistant.StockCardService.WebApi.Mappers
{
    /// <summary>
    /// Маппер для карточки акции model/dto.
    /// </summary>
    public static class ShareCardMapper
    {
        /// <summary>
        /// Конвертирует модель карточки акции в DTO.
        /// </summary>
        /// <param name="model">Исходная модель карточки акции.</param>
        /// <returns>DTO карточки акции.</returns>
        public static ShareCardDto ToDto(ShareCardModel model)
        {
            return new ShareCardDto
            {
                Id = model.Id,
                Name = model.Name,
                Ticker = model.Ticker,
                Description = model.Description,
                Currency = model.Currency,
                CurrentPrice = model.CurrentPrice,
                Dividends = model.Dividends.Select(DividendMapper.ToDto).ToList(),
                Multipliers = model.Multipliers.Select(MultiplierMapper.ToDto).ToList(),
                FinancialReports = model.FinancialReports.Select(FinancialReportMapper.ToDto).ToList()
            };
        }

        /// <summary>
        /// Конвертирует модель создаваемой карточки акции в DTO.
        /// </summary>
        /// <param name="model">Исходная модель создаваемой карточки акции.</param>
        /// <returns>DTO создаваемой карточки акции.</returns>
        public static CreatingShareCardDto ToDto(CreatingShareCardModel model)
        {
            return new CreatingShareCardDto
            {
                Name = model.Name,
                Ticker = model.Ticker,
                Description = model.Description,
                Currency = model.Currency,
            };
        }

        /// <summary>
        /// Конвертирует модель изменяемой карточки акции в DTO.
        /// </summary>
        /// <param name="model">Исходная модель изменяемой карточки акции.</param>
        /// <returns>DTO изменяемой карточки акции.</returns>
        public static UpdatingShareCardDto ToDto(UpdatingShareCardModel model)
        {
            return new UpdatingShareCardDto
            {
                Id = model.Id,
                Name = model.Name,
                Ticker = model.Ticker,
                CurrentPrice = model.CurrentPrice,
                Description = model.Description
            };
        }

        /// <summary>
        /// Конвертирует модель краткой карточки акции в DTO.
        /// </summary>
        /// <param name="model">Исходная модель краткой карточки акции.</param>
        /// <returns>DTO краткой карточки акции.</returns>
        public static ShareCardShortDto ToDto(ShareCardShortModel model)
        {
            return new ShareCardShortDto
            {
                Id = model.Id,
                Name = model.Name,
                Ticker = model.Ticker,
                Description = model.Description,
                CurrentPrice = model.CurrentPrice,
                Currency = model.Currency
            };
        }

        /// <summary>
        /// Конвертирует DTO карточки акции в модель.
        /// </summary>
        /// <param name="dto">Исходный DTO карточки акции.</param>
        /// <returns>Модель карточки акции.</returns>
        public static ShareCardModel ToModel(ShareCardDto dto)
        {
            return new ShareCardModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Ticker = dto.Ticker,
                Description = dto.Description,
                Currency = dto.Currency,
                CurrentPrice = dto.CurrentPrice,
                Dividends = dto.Dividends.Select(DividendMapper.ToModel).ToList(),
                Multipliers = dto.Multipliers.Select(MultiplierMapper.ToModel).ToList(),
                FinancialReports = dto.FinancialReports.Select(FinancialReportMapper.ToModel).ToList()
            };
        }

        /// <summary>
        /// Конвертирует DTO создаваемой карточки акции в модель.
        /// </summary>
        /// <param name="dto">Исходный DTO создаваемой карточки акции.</param>
        /// <returns>Модель создаваемой карточки акции.</returns>
        public static CreatingShareCardModel ToModel(CreatingShareCardDto dto)
        {
            return new CreatingShareCardModel
            {
                Name = dto.Name,
                Ticker = dto.Ticker,
                Description = dto.Description,
                Currency = dto.Currency
            };
        }

        /// <summary>
        /// Конвертирует DTO изменяемой карточки акции в модель.
        /// </summary>
        /// <param name="dto">Исходный DTO изменяемой карточки акции.</param>
        /// <returns>Модель изменяемой карточки акции.</returns>
        public static UpdatingShareCardModel ToModel(UpdatingShareCardDto dto)
        {
            return new UpdatingShareCardModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Ticker = dto.Ticker,
                CurrentPrice = dto.CurrentPrice,
                Description = dto.Description
            };
        }

        /// <summary>
        /// Конвертирует DTO краткой карточки акции в модель.
        /// </summary>
        /// <param name="dto">Исходный DTO краткой карточки акции.</param>
        /// <returns>Модель краткой карточки акции.</returns>
        public static ShareCardShortModel ToModel(ShareCardShortDto dto)
        {
            return new ShareCardShortModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Ticker = dto.Ticker,
                Description = dto.Description,
                CurrentPrice = dto.CurrentPrice,
                Currency = dto.Currency
            };
        }
    }
}