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
    public class Portfolio(Guid id, Guid userId)
                : BaseEntity<Guid>(id)
    {

        /// <summary>
        /// Идентификатор пользователя-владельца портфеля
        /// </summary>
        public Guid UserId { get; protected set; } = userId;

        /// <summary>
        /// Наименование портфеля
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Валюта портфеля (USD, RUB и т.д.)
        /// </summary>
        public required string Currency{ get; set; }

        /// <summary>
        /// Набор активов в портфеле
        /// </summary>
        public virtual ICollection<PortfolioAsset> Assets { get; set; } = [];


        /// <summary>
        /// Не предполагается хранить в БД, а только кэшировать в Redis
        /// </summary>

        //public decimal TotalPrice { get; private set; }

        /// <summary>
        /// Не предполагается хранить в БД, а только кэшировать в Redis
        /// </summary>

        //public DateTime LastUpdated { get; private set; }

    }
}
