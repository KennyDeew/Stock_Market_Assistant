namespace StockMarketAssistant.PortfolioService.Domain.Entities
{
    /// <summary>
    /// Доменная сущность актива из портфеля
    /// </summary>
    public class PortfolioAsset(Guid id, Guid portfolioId, Guid stockCardId, PortfolioAssetType assetType) 
        : BaseEntity<Guid>(id)
    {
        /// <summary>
        /// Тип актива
        /// </summary>
        public PortfolioAssetType AssetType { get; protected set; } = assetType;

        /// <summary>
        /// Id карточки актива
        /// </summary>
        public Guid StockCardId { get; protected set; } = stockCardId;

        /// <summary>
        /// Id портфеля
        /// </summary>
        public Guid PortfolioId { get; protected set; } = portfolioId;

        /// <summary>
        /// Портфель
        /// </summary>
        public virtual Portfolio? Portfolio { get; set; }

        /// <summary>
        /// Количество, шт.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Средняя цена покупки
        /// </summary>
        public decimal? AveragePurchasePrice { get; set; }

        /// <summary>
        /// Дата последнего обновления информации об активе
        /// </summary>
        public DateTime? LastUpdated { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        public string? Currency { get; set; }
    }
}
