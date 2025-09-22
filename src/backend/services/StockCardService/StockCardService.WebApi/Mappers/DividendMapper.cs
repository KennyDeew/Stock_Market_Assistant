using StockCardService.WebApi.Models._01sub_Dividend;
using StockMarketAssistant.StockCardService.Application.DTOs._01sub_Dividend;

namespace StockMarketAssistant.StockCardService.WebApi.Mappers
{
    /// <summary>
    /// Маппер для дивидендов model/dto
    /// </summary>
    public static class DividendMapper
    {
        /// <summary>
        /// Конвертирует модель дивиденда в DTO.
        /// </summary>
        /// <param name="model">Исходная модель дивиденда.</param>
        /// <returns>Объект DTO с данными из модели.</returns>
        public static DividendDto ToDto(DividendModel model)
        {
            return new DividendDto
            {
                Id = model.Id,
                ShareCardId = model.ShareCardId,
                CutOffDate = DateTime.SpecifyKind(model.CutOffDate, DateTimeKind.Utc),
                Period = model.Period,
                Currency = model.Currency,
                Value = model.Value
            };
        }

        /// <summary>
        /// Конвертирует модель создаваемого дивиденда в DTO.
        /// </summary>
        /// <param name="model">Исходная модель создаваемого дивиденда.</param>
        /// <returns>Объект DTO с данными из модели.</returns>
        public static CreatingDividendDto ToDto(CreatingDividendModel model)
        {
            return new CreatingDividendDto
            {
                ShareCardId = model.ShareCardId,
                CutOffDate = DateTime.SpecifyKind(model.CutOffDate, DateTimeKind.Utc),
                Period = model.Period,
                Currency = model.Currency,
                Value = model.Value
            };
        }

        /// <summary>
        /// Конвертирует модель изменяемого дивиденда в DTO.
        /// </summary>
        /// <param name="model">Исходная модель изменяемого дивиденда.</param>
        /// <returns>Объект DTO с данными из модели.</returns>
        public static UpdatingDividendDto ToDto(UpdatingDividendModel model)
        {
            return new UpdatingDividendDto
            {
                CutOffDate = DateTime.SpecifyKind(model.CutOffDate, DateTimeKind.Utc),
                Value = model.Value
            };
        }

        /// <summary>
        /// Конвертирует Dto дивиденда в модель.
        /// </summary>
        /// <param name="dto">Исходный dto дивиденда.</param>
        /// <returns>Модель дивиденда с данными из Dto.</returns>
        public static DividendModel ToModel(DividendDto dto)
        {
            return new DividendModel
            {
                Id = dto.Id,
                ShareCardId = dto.ShareCardId,
                CutOffDate = dto.CutOffDate,
                Period = dto.Period,
                Currency = dto.Currency,
                Value = dto.Value
            };
        }

        /// <summary>
        /// Конвертирует Dto создаваемого дивиденда в модель.
        /// </summary>
        /// <param name="dto">Исходный dto создаваемого дивиденда.</param>
        /// <returns>Модель создаваемого дивиденда с данными из Dto.</returns>
        public static CreatingDividendModel ToDto(CreatingDividendDto dto)
        {
            return new CreatingDividendModel
            {
                ShareCardId = dto.ShareCardId,
                CutOffDate = dto.CutOffDate,
                Period = dto.Period,
                Currency = dto.Currency,
                Value = dto.Value
            };
        }

        /// <summary>
        /// Конвертирует Dto изменяемого дивиденда в модель.
        /// </summary>
        /// <param name="dto">Исходный dto изменяемого дивиденда.</param>
        /// <returns>Модель изменяемого дивиденда с данными из Dto.</returns>
        public static UpdatingDividendModel ToDto(UpdatingDividendDto dto)
        {
            return new UpdatingDividendModel
            {
                CutOffDate = dto.CutOffDate,
                Value = dto.Value
            };
        }
    }
}
