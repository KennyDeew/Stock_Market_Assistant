using StockMarketAssistant.PortfolioService.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockMarketAssistant.PortfolioService.Domain.Entities
{
    /// <summary>
    /// Доменная сущность актива из портфеля
    /// </summary>
    /// <remarks>
    /// Конструктор
    /// </remarks>
    /// <param name="id">Id актива</param>
    /// <param name="portfolioId">Id портфеля</param>
    /// <param name="stockCardId">Id карточки актива</param>
    /// <param name="assetType">Тип актива</param>
    public class PortfolioAsset(Guid id, Guid portfolioId, Guid stockCardId, PortfolioAssetType assetType) 
        : BaseEntity<Guid>(id)
    {
        /// <summary>
        /// Id портфеля
        /// </summary>
        public Guid PortfolioId { get; protected set; } = portfolioId;

        /// <summary>
        /// Портфель
        /// </summary>
        public virtual Portfolio? Portfolio { get; set; }

        /// <summary>
        /// Id карточки актива
        /// </summary>
        public Guid StockCardId { get; protected set; } = stockCardId;

        /// <summary>
        /// Тип актива
        /// </summary>
        public PortfolioAssetType AssetType { get; protected set; } = assetType;

        /// <summary>
        /// Набор торговых операций по активу
        /// </summary>
        public virtual ICollection<PortfolioAssetTransaction> Transactions { get; } = [];

        /// <summary>
        /// Общее количество актива (вычисляемое свойство)
        /// </summary>
        [NotMapped] // Поле не должно сохраняться в БД
        public int TotalQuantity
        {
            get
            {
                if (Transactions == null || Transactions.Count == 0)
                    return 0;

                return Transactions.Sum(transaction =>
                    transaction.TransactionType == PortfolioAssetTransactionType.Buy
                        ? transaction.Quantity
                        : -transaction.Quantity);
            }
        }

        /// <summary>
        /// Средняя цена покупки актива (вычисляемое свойство)
        /// </summary>
        [NotMapped]
        public decimal AveragePurchasePrice
        {
            get
            {
                if (Transactions == null || Transactions.Count == 0)
                    return 0;

                var buyTransactions = Transactions
                    .Where(t => t.TransactionType == PortfolioAssetTransactionType.Buy)
                    .ToList();

                if (buyTransactions.Count == 0)
                    return 0;

                decimal totalCost = buyTransactions.Sum(t => t.Quantity * t.PricePerUnit);
                int totalQuantity = buyTransactions.Sum(t => t.Quantity);

                return totalQuantity > 0 ? totalCost / totalQuantity : 0;
            }
        }

        /// <summary>
        /// Общая сумма инвестиций в актив (вычисляемое свойство)
        /// </summary>
        [NotMapped] // Поле не должно сохраняться в БД
        public decimal TotalInvestment
        {
            get
            {
                if (Transactions == null || Transactions.Count == 0)
                    return 0;

                return Transactions
                    .Where(t => t.TransactionType == PortfolioAssetTransactionType.Buy)
                    .Sum(t => t.Quantity * t.PricePerUnit);
            }
        }

        /// <summary>
        /// Дата последнего обновления актива (вычисляемое свойство)
        /// </summary>
        [NotMapped]
        public DateTime LastUpdated
        {
            get
            {
                if (Transactions == null || Transactions.Count == 0)
                    return DateTime.MinValue;

                return Transactions.Max(t => t.TransactionDate);
            }
        }
    }
}
