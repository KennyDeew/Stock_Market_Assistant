using StockCardService.WebApi.Models._01sub_FinancialReport;
using StockMarketAssistant.StockCardService.Application.DTOs._01sub_FinancialReport;

namespace StockMarketAssistant.StockCardService.WebApi.Mappers
{
    /// <summary>
    /// Маппер для финансового отчета model/dto 
    /// </summary>
    public static class FinancialReportMapper
    {
        /// <summary>
        /// Конвертирует модель финансового отчета в DTO.
        /// </summary>
        /// <param name="model">Исходная модель отчета.</param>
        /// <returns>Объект DTO с данными из модели.</returns>
        public static FinancialReportDto ToDto(FinancialReportModel model)
        {
            return new FinancialReportDto
            {
                Id = model.Id,
                ParentId = model.ParentId,
                Name = model.Name,
                Description = model.Description,
                Period = model.Period,
                Revenue = model.Revenue,
                EBITDA = model.EBITDA,
                NetProfit = model.NetProfit,
                CAPEX = model.CAPEX,
                FCF = model.FCF,
                Debt = model.Debt,
                TotalAssets = model.TotalAssets,
                NonCurrentAssets = model.NonCurrentAssets,
                CurrentAssets = model.CurrentAssets,
                Inventories = model.Inventories,
                AccountsReceivable = model.AccountsReceivable,
                CashAndEquivalents = model.CashAndEquivalents,
                NonCurrentLiabilities = model.NonCurrentLiabilities,
                CurrentLiabilities = model.CurrentLiabilities
            };
        }

        /// <summary>
        /// Конвертирует модель создаваемого финансового отчета в DTO.
        /// </summary>
        /// <param name="model">Исходная модель создаваемого отчета.</param>
        /// <returns>Объект DTO с данными из модели.</returns>
        public static CreatingFinancialReportDto ToDto(CreatingFinancialReportModel model)
        {
            return new CreatingFinancialReportDto
            {
                ParentId = model.ParentId,
                Name = model.Name,
                Description = model.Description,
                Period = model.Period,
                Revenue = model.Revenue,
                EBITDA = model.EBITDA,
                NetProfit = model.NetProfit,
                CAPEX = model.CAPEX,
                FCF = model.FCF,
                Debt = model.Debt,
                TotalAssets = model.TotalAssets,
                NonCurrentAssets = model.NonCurrentAssets,
                CurrentAssets = model.CurrentAssets,
                Inventories = model.Inventories,
                AccountsReceivable = model.AccountsReceivable,
                CashAndEquivalents = model.CashAndEquivalents,
                NonCurrentLiabilities = model.NonCurrentLiabilities,
                CurrentLiabilities = model.CurrentLiabilities
            };
        }

        /// <summary>
        /// Конвертирует модель изменяемого финансового отчета в DTO.
        /// </summary>
        /// <param name="model">Исходная модель изменяемого отчета.</param>
        /// <returns>Объект DTO с данными из модели.</returns>
        public static UpdatingFinancialReportDto ToDto(UpdatingFinancialReportModel model)
        {
            return new UpdatingFinancialReportDto
            {
                Id = model.Id,
                ParentId = model.ParentId,
                Name = model.Name,
                Description = model.Description,
                Period = model.Period,
                Revenue = model.Revenue,
                EBITDA = model.EBITDA,
                NetProfit = model.NetProfit,
                CAPEX = model.CAPEX,
                FCF = model.FCF,
                Debt = model.Debt,
                TotalAssets = model.TotalAssets,
                NonCurrentAssets = model.NonCurrentAssets,
                CurrentAssets = model.CurrentAssets,
                Inventories = model.Inventories,
                AccountsReceivable = model.AccountsReceivable,
                CashAndEquivalents = model.CashAndEquivalents,
                NonCurrentLiabilities = model.NonCurrentLiabilities,
                CurrentLiabilities = model.CurrentLiabilities
            };
        }

        /// <summary>
        /// Конвертирует Dto финансового отчета в модель.
        /// </summary>
        /// <param name="dto">Исходный dto отчета.</param>
        /// <returns>Модель отчета с данными из Dto.</returns>
        public static FinancialReportModel ToModel(FinancialReportDto dto)
        {
            return new FinancialReportModel
            {
                Id = dto.Id,
                ParentId = dto.ParentId,
                Name = dto.Name,
                Description = dto.Description,
                Period = dto.Period,
                Revenue = dto.Revenue,
                EBITDA = dto.EBITDA,
                NetProfit = dto.NetProfit,
                CAPEX = dto.CAPEX,
                FCF = dto.FCF,
                Debt = dto.Debt,
                TotalAssets = dto.TotalAssets,
                NonCurrentAssets = dto.NonCurrentAssets,
                CurrentAssets = dto.CurrentAssets,
                Inventories = dto.Inventories,
                AccountsReceivable = dto.AccountsReceivable,
                CashAndEquivalents = dto.CashAndEquivalents,
                NonCurrentLiabilities = dto.NonCurrentLiabilities,
                CurrentLiabilities = dto.CurrentLiabilities
            };
        }

        /// <summary>
        /// Конвертирует Dto создаваемого финансового отчета в модель.
        /// </summary>
        /// <param name="dto">Исходный dto отчета.</param>
        /// <returns>Модель создаваемого отчета с данными из Dto.</returns>
        public static CreatingFinancialReportModel ToModel(CreatingFinancialReportDto dto)
        {
            return new CreatingFinancialReportModel
            {
                ParentId = dto.ParentId,
                Name = dto.Name,
                Description = dto.Description,
                Period = dto.Period,
                Revenue = dto.Revenue,
                EBITDA = dto.EBITDA,
                NetProfit = dto.NetProfit,
                CAPEX = dto.CAPEX,
                FCF = dto.FCF,
                Debt = dto.Debt,
                TotalAssets = dto.TotalAssets,
                NonCurrentAssets = dto.NonCurrentAssets,
                CurrentAssets = dto.CurrentAssets,
                Inventories = dto.Inventories,
                AccountsReceivable = dto.AccountsReceivable,
                CashAndEquivalents = dto.CashAndEquivalents,
                NonCurrentLiabilities = dto.NonCurrentLiabilities,
                CurrentLiabilities = dto.CurrentLiabilities
            };
        }

        /// <summary>
        /// Конвертирует Dto изменяемого финансового отчета в модель.
        /// </summary>
        /// <param name="dto">Исходный dto отчета.</param>
        /// <returns>Модель изменяемого отчета с данными из Dto.</returns>
        public static UpdatingFinancialReportModel ToModel(UpdatingFinancialReportDto dto)
        {
            return new UpdatingFinancialReportModel
            {
                Id = dto.Id,
                ParentId = dto.ParentId,
                Name = dto.Name,
                Description = dto.Description,
                Period = dto.Period,
                Revenue = dto.Revenue,
                EBITDA = dto.EBITDA,
                NetProfit = dto.NetProfit,
                CAPEX = dto.CAPEX,
                FCF = dto.FCF,
                Debt = dto.Debt,
                TotalAssets = dto.TotalAssets,
                NonCurrentAssets = dto.NonCurrentAssets,
                CurrentAssets = dto.CurrentAssets,
                Inventories = dto.Inventories,
                AccountsReceivable = dto.AccountsReceivable,
                CashAndEquivalents = dto.CashAndEquivalents,
                NonCurrentLiabilities = dto.NonCurrentLiabilities,
                CurrentLiabilities = dto.CurrentLiabilities
            };
        }
    }
}
