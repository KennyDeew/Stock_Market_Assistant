using System.ComponentModel.DataAnnotations.Schema;

namespace StockMarketAssistant.PortfolioService.Domain.Entities
{
    /// <summary>
    /// Доменная сущность портфеля ценных бумаг
    /// </summary>
    /// <remarks>
    /// Конструктор
    /// </remarks>
    /// <param name="id">Id портфеля</param>
    /// <param name="userId">Id пользователя</param>
    public class Portfolio(Guid id, Guid userId, string name, string currency = "RUB")
                : BaseEntity<Guid>(id)
    {

        /// <summary>
        /// Идентификатор пользователя-владельца портфеля
        /// </summary>
        public Guid UserId { get; protected set; } = userId;

        /// <summary>
        /// Наименование портфеля
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// Валюта портфеля (RUB, USD и т.д.)
        /// </summary>
        public string Currency { get; set; } = currency;

        /// <summary>
        /// Набор активов в портфеле
        /// </summary>
        public virtual ICollection<PortfolioAsset> Assets { get; set; } = [];

        /// <summary>
        /// Итоговая цена портфеля
        /// </summary>
        [NotMapped]
        public decimal TotalPrice
        {
            get
            {
                if (Assets == null || Assets.Count == 0)
                    return 0;
                return Assets.Sum(a => a.TotalQuantity * a.AveragePurchasePrice);
            }
        }

        /// <summary>
        /// Дата последнего обновления портфеля (вычисляемое свойство)
        /// </summary>
        [NotMapped]
        public DateTime LastUpdated
        {
            get
            {
                if (Assets == null || Assets.Count == 0)
                    return DateTime.MinValue;

                return Assets.Max(a => a.LastUpdated);
            }
        }
    }
}
