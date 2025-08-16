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
                Id = model.Id,
                ShareCardId = model.ShareCardId,
                CuttOffDate = DateTime.SpecifyKind(model.CuttOffDate, DateTimeKind.Utc),
                Period = model.Period,
                Currency = model.Currency,
                Value = model.Value
            };
        }

        public static CreatingDividendDto ToDto(CreatingDividendModel model)
        {
            if (model == null) return null;

            return new CreatingDividendDto
            {
                ShareCardId = model.ShareCardId,
                CuttOffDate = DateTime.SpecifyKind(model.CuttOffDate, DateTimeKind.Utc),
                Period = model.Period,
                Currency = model.Currency,
                Value = model.Value
            };
        }

        public static UpdatingDividendDto ToDto(UpdatingDividendModel model)
        {
            if (model == null) return null;

            return new UpdatingDividendDto
            {
                CuttOffDate = DateTime.SpecifyKind(model.CuttOffDate, DateTimeKind.Utc),
                Value = model.Value
            };
        }

        public static DividendModel ToModel(DividendDto dto)
        {
            if (dto == null) return null;

            return new DividendModel
            {
                Id = dto.Id,
                ShareCardId = dto.ShareCardId,
                CuttOffDate = dto.CuttOffDate,
                Period = dto.Period,
                Currency = dto.Currency,
                Value = dto.Value
            };
        }

        public static CreatingDividendModel ToDto(CreatingDividendDto dto)
        {
            if (dto == null) return null;

            return new CreatingDividendModel
            {
                ShareCardId = dto.ShareCardId,
                CuttOffDate = dto.CuttOffDate,
                Period = dto.Period,
                Currency = dto.Currency,
                Value = dto.Value
            };
        }

        public static UpdatingDividendModel ToDto(UpdatingDividendDto dto)
        {
            if (dto == null) return null;

            return new UpdatingDividendModel
            {
                CuttOffDate = dto.CuttOffDate,
                Value = dto.Value
            };
        }
    }
}
