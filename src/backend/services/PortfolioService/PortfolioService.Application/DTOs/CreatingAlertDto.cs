using StockMarketAssistant.PortfolioService.Domain.Enums;

namespace StockMarketAssistant.PortfolioService.Application.DTOs
{
    /// <summary>
    /// DTO для создания уведомления пользователя о достижении цены
    /// </summary>
    /// <param name="StockCardId">Идентификатор карточки актива</param>
    /// <param name="AssetType">Тип актива</param>
    /// <param name="AssetTicker">Тикер</param>
    /// <param name="AssetName">Название</param>
    /// <param name="TargetPrice">Целевая цена</param>
    /// <param name="AssetCurrency">Валюта актива(RUB, USD и т.д.)</param>
    /// <param name="Condition">Условие</param>
    public record CreatingAlertDto(
        Guid StockCardId,
        PortfolioAssetType AssetType,
        string AssetTicker,
        string AssetName,
        decimal TargetPrice,
        string AssetCurrency,
        AlertCondition Condition);
}