using StockCardService.WebApi.Models._01sub_Dividend;
using StockMarketAssistant.StockCardService.Application.DTOs._01sub_Dividend;

namespace StockMarketAssistant.StockCardService.WebApi.Mappers
{
    public static class DividendMapper
    {
        public static DividendDto ToDto(DividendModel model)
        {
            if (model == null) return null;

            return new DividendDto
            {
                Id = model.Id
            };
        }

        public static DividendModel ToModel(DividendDto dto)
        {
            if (dto == null) return null;

            return new DividendModel
            {
                Id = dto.Id
            };
        }
         
         
         
    }
}
