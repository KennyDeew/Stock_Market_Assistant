using StockCardService.WebApi.Models._01sub_Multiplier;
using StockMarketAssistant.StockCardService.Application.DTOs._01sub_Multiplier;

namespace StockMarketAssistant.StockCardService.WebApi.Mappers
{
    public static class MultiplierMapper
    {
        public static MultiplierDto ToDto(MultiplierModel model)
        {
            if (model == null) return null;

            return new MultiplierDto
            {
                Id = model.Id,
                Name = model.Name
            };
        }

        public static MultiplierModel ToModel(MultiplierDto dto)
        {
            if (dto == null) return null;

            return new MultiplierModel
            {
                Id = dto.Id,
                Name = dto.Name
            };
        }
    }
}
