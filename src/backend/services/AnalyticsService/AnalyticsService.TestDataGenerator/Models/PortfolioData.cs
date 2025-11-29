namespace AnalyticsService.TestDataGenerator.Models;

/// <summary>
/// Данные портфеля для генерации
/// </summary>
public class PortfolioData
{
    /// <summary>
    /// Идентификатор портфеля
    /// </summary>
    public Guid PortfolioId { get; set; }

    /// <summary>
    /// Название портфеля
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Список активов в портфеле
    /// </summary>
    public List<Guid> AssetIds { get; set; } = new();
}


