using StockCardService.WebApi.Models._01sub_FinancialReport;
using StockMarketAssistant.StockCardService.Application.DTOs._01sub_FinancialReport;

namespace StockMarketAssistant.StockCardService.WebApi.Mappers
{
    public static class FinancialReportMapper
    {
        
         public static FinancialReportDto ToDto(FinancialReportModel model)
        {
            if (model == null) return null;

            return new FinancialReportDto
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description
            };
        }

        public static FinancialReportModel ToModel(FinancialReportDto dto)
        {
            if (dto == null) return null;

            return new FinancialReportModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description
            };
        }
         
    }
}
