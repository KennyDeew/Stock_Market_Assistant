using StockCardService.Domain.Entities;

namespace StockMarketAssistant.StockCardService.Domain.Entities

{
    public class Coupon : IEntityWithParent<Guid>, IEntity<Guid>
    {
        /// <summary>
        /// Id купона
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Id облигации
        /// </summary>
        public Guid ParentId { get; set; }

        /// <summary>
        /// дата фиксирования владельца облигацией для выплаты дивиденда
        /// </summary>
        public DateTime CutOffDate { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Размер купона
        /// </summary>
        public decimal Value { get; set; }

        public BondCard Bond { get; set; }
    }
}

