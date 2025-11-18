using StockCardService.WebApi.Models._01sub_Multiplier;
using StockMarketAssistant.StockCardService.Application.DTOs._01sub_Multiplier;

namespace StockMarketAssistant.StockCardService.WebApi.Mappers
{
    /// <summary>
    /// Маппер для мельтипликаторов акции model/dto.
    /// </summary>
    public static class MultiplierMapper
    {
        /// <summary>
        /// Конвертирует модель мельтипликатора акции в DTO.
        /// </summary>
        /// <param name="model">Исходная модель мельтипликатора акции.</param>
        /// <returns>DTO мельтипликатора акции.</returns>
        public static MultiplierDto ToDto(MultiplierModel model)
        {
            return new MultiplierDto
            {
                Id = model.Id,
                Name = model.Name
            };
        }

        /// <summary>
        /// Конвертирует DTO мельтипликатора акции в модель.
        /// </summary>
        /// <param name="dto">Исходный DTO мельтипликатора акции.</param>
        /// <returns>Модель мельтипликатора акции.</returns>
        public static MultiplierModel ToModel(MultiplierDto dto)
        {
            return new MultiplierModel
            {
                Id = dto.Id,
                Name = dto.Name
            };
        }
    }
}
