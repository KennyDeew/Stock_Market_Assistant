namespace AnalyticsService.TestDataGenerator.Models;

/// <summary>
/// Модель компании для генерации тестовых данных
/// </summary>
public class CompanyAsset
{
    /// <summary>
    /// Идентификатор актива (StockCardId)
    /// </summary>
    public Guid StockCardId { get; set; }

    /// <summary>
    /// Тикер компании
    /// </summary>
    public required string Ticker { get; set; }

    /// <summary>
    /// Название компании
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Базовая цена акции (для генерации случайных цен)
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Минимальная цена (для генерации)
    /// </summary>
    public decimal MinPrice { get; set; }

    /// <summary>
    /// Максимальная цена (для генерации)
    /// </summary>
    public decimal MaxPrice { get; set; }
}


