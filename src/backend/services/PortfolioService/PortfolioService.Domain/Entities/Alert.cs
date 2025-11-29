using StockMarketAssistant.PortfolioService.Domain.Enums;
namespace StockMarketAssistant.PortfolioService.Domain.Entities
{
    /// <summary>
    /// Уведомление пользователя о достижении цены
    /// </summary>
    /// Конструктор
    /// </summary>
    /// <param name="id">Идентификатор</param>
    /// <param name="stockCardId">Идентификатор карточки актива</param>
    /// <param name="assetType">Тип актива</param>
    /// <param name="assetTicker">Тикер</param>
    /// <param name="assetName">Название</param>
    /// <param name="targetPrice">Целевая цена</param>
    /// <param name="assetCurrency">Валюта актива</param>
    /// <param name="condition">Условие</param>
    /// <param name="userId">Владелец</param>
    /// <param name="userEmail">Email для уведомления</param>
    public class Alert(
        Guid id,
        Guid stockCardId,
        PortfolioAssetType assetType,
        string assetTicker,
        string assetName,
        decimal targetPrice,
        string assetCurrency,
        AlertCondition condition,
        Guid userId,
        string userEmail) : BaseEntity<Guid>(id)
    {
        /// <summary>
        /// Идентификатор актива в портфеле
        /// </summary>
        public Guid StockCardId { get; private set; } = stockCardId;

        /// <summary>
        /// Тип актива (акция, облигация и т.д.)
        /// </summary>
        public PortfolioAssetType AssetType { get; private set; } = assetType;

        /// <summary>
        /// Тикер актива (например, AAPL)
        /// </summary>
        public string AssetTicker { get; private set; } = assetTicker;

        /// <summary>
        /// Название актива
        /// </summary>
        public string AssetName { get; private set; } = assetName;

        /// <summary>
        /// Целевая цена
        /// </summary>
        public decimal TargetPrice { get; private set; } = targetPrice;

        /// <summary>
        /// Валюта актива (RUB, USD и т.д.)
        /// </summary>
        public string AssetCurrency { get; private set; } = assetCurrency;

        /// <summary>
        /// Условие: выше или ниже
        /// </summary>
        public AlertCondition Condition { get; private set; } = condition;

        /// <summary>
        /// Активно ли уведомление
        /// </summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// Время создания
        /// </summary>
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Время последнего обновления
        /// </summary>
        public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Время срабатывания (если сработало)
        /// </summary>
        public DateTimeOffset? TriggeredAt { get; private set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public Guid UserId { get; private set; } = userId;

        /// <summary>
        /// Email пользователя
        /// </summary>
        public string UserEmail { get; private set; } = userEmail;

        /// <summary>
        /// Время последней проверки цены
        /// </summary>
        public DateTimeOffset? LastChecked { get; private set; }

        /// <summary>
        /// Деактивировать уведомление (например, после срабатывания)
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Отметить как сработавшее
        /// </summary>
        public void MarkAsTriggered()
        {
            TriggeredAt = DateTimeOffset.UtcNow;
            Deactivate();
        }

        /// <summary>
        /// Обновить время последней проверки
        /// </summary>
        public void UpdateLastChecked()
        {
            LastChecked = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
