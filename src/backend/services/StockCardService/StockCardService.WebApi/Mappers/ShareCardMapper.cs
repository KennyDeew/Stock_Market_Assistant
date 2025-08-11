using StockCardService.WebApi.Models.ShareCard;
using StockMarketAssistant.StockCardService.Application.DTOs._01_ShareCard;

namespace StockMarketAssistant.StockCardService.WebApi.Mappers
{
    public static class ShareCardMapper
    {
        public static ShareCardDto ToDto(ShareCardModel model)
        {
            if (model == null) return null;

            return new ShareCardDto
            {
                Id = model.Id,
                Name = model.Name,
                Ticker = model.Ticker,
                Description = model.Description,
                Dividends = model.Dividends.Select(DividendMapper.ToDto).ToList(),
                Multipliers = model.Multipliers.Select(MultiplierMapper.ToDto).ToList(),
                FinancialReports = model.FinancialReports.Select(FinancialReportMapper.ToDto).ToList()
            };
        }

        public static CreatingShareCardDto ToDto(CreatingShareCardModel model)
        {
            if (model == null) return null;

            return new CreatingShareCardDto
            {
                Name = model.Name,
                Ticker = model.Ticker,
                Description = model.Description
            };
        }

        public static UpdatingShareCardDto ToDto(UpdatingShareCardModel model)
        {
            if (model == null) return null;

            return new UpdatingShareCardDto
            {
                Id = model.Id,
                Name = model.Name,
                Ticker = model.Ticker,
                Description = model.Description
            };
        }

        public static ShareCardShortDto ToDto(ShareCardShortModel model)
        {
            if (model == null) return null;

            return new ShareCardShortDto
            {
                Id = model.Id,
                Name = model.Name,
                Ticker = model.Ticker,
                Description = model.Description
            };
        }

        public static ShareCardModel ToModel(ShareCardDto dto)
        {
            if (dto == null) return null;

            return new ShareCardModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Ticker = dto.Ticker,
                Description = dto.Description,
                Dividends = dto.Dividends.Select(DividendMapper.ToModel).ToList(),
                Multipliers = dto.Multipliers.Select(MultiplierMapper.ToModel).ToList(),
                FinancialReports = dto.FinancialReports.Select(FinancialReportMapper.ToModel).ToList()
            };
        }

        public static CreatingShareCardModel ToModel(CreatingShareCardDto dto)
        {
            if (dto == null) return null;

            return new CreatingShareCardModel
            {
                Name = dto.Name,
                Ticker = dto.Ticker,
                Description = dto.Description
            };
        }

        public static UpdatingShareCardModel ToModel(UpdatingShareCardDto dto)
        {
            if (dto == null) return null;

            return new UpdatingShareCardModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Ticker = dto.Ticker,
                Description = dto.Description
            };
        }

        public static ShareCardShortModel ToModel(ShareCardShortDto dto)
        {
            if (dto == null) return null;

            return new ShareCardShortModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Ticker = dto.Ticker,
                Description = dto.Description
            };
        }
    }
}
