using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.WebApi.Models;
using StockMarketAssistant.SharedLibrary.Enums;

namespace StockMarketAssistant.PortfolioService.WebApi.Mappings
{
    /// <summary>
    /// Маппинг между DTO приложения и моделями контроллера
    /// </summary>
    public static class ProfitLossMapping
    {
        /// <summary>
        /// Преобразование DTO доходности портфеля в модель ответа
        /// </summary>
        public static PortfolioProfitLossResponse ToResponse(this PortfolioProfitLossDto dto)
        {
            return new PortfolioProfitLossResponse
            {
                PortfolioId = dto.PortfolioId,
                PortfolioName = dto.PortfolioName,
                TotalAbsoluteReturn = dto.TotalAbsoluteReturn,
                TotalPercentageReturn = dto.TotalPercentageReturn,
                TotalInvestment = dto.TotalInvestment,
                TotalCurrentValue = dto.TotalCurrentValue,
                BaseCurrency = dto.BaseCurrency,
                CalculatedAt = dto.CalculatedAt,
                AssetBreakdown = [.. dto.AssetBreakdown.Select(ToResponse)]
            };
        }

        /// <summary>
        /// Преобразование DTO доходности актива в модель ответа
        /// </summary>
        public static PortfolioAssetProfitLossItemResponse ToResponse(this PortfolioAssetProfitLossItemDto dto)
        {
            return new PortfolioAssetProfitLossItemResponse
            {
                AssetId = dto.AssetId,
                Ticker = dto.Ticker,
                AssetName = dto.AssetName,
                AbsoluteReturn = dto.AbsoluteReturn,
                PercentageReturn = dto.PercentageReturn,
                InvestmentAmount = dto.InvestmentAmount,
                CurrentValue = dto.CurrentValue ?? 0,
                Currency = dto.Currency,
                Quantity = dto.Quantity,
                AveragePurchasePrice = dto.AveragePurchasePrice,
                CurrentPrice = dto.CurrentPrice ?? 0,
                WeightInPortfolio = dto.WeightInPortfolio ?? 0
            };
        }

        /// <summary>
        /// Преобразование DTO доходности отдельного актива в модель ответа
        /// </summary>
        public static PortfolioAssetProfitLossResponse ToResponse(this PortfolioAssetProfitLossDto dto)
        {
            return new PortfolioAssetProfitLossResponse
            {
                AssetId = dto.AssetId,
                PortfolioId = dto.PortfolioId,
                Ticker = dto.Ticker,
                AssetName = dto.AssetName,
                AbsoluteReturn = dto.AbsoluteReturn ?? 0,
                PercentageReturn = dto.PercentageReturn ?? 0,
                InvestmentAmount = dto.InvestmentAmount,
                CurrentValue = dto.CurrentValue ?? 0,
                Currency = dto.Currency,
                Quantity = dto.Quantity,
                AveragePurchasePrice = dto.AveragePurchasePrice,
                CurrentPrice = dto.CurrentPrice ?? 0,
                CalculationType = dto.CalculationType
            };
        }
    }
}
