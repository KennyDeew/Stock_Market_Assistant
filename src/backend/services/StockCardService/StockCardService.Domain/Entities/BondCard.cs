using StockCardService.Domain.Entities;

namespace StockMarketAssistant.StockCardService.Domain.Entities

{
    public class BondCard : IEntity<Guid>
    {
        /// <summary>
        /// Id облигации
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Тикер облигации
        /// </summary>
        public string Ticker { get; set; }

        /// <summary>
        /// Наименование облигации
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание облигации
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// дата погашения облигации
        /// </summary>
        public DateTime MaturityPeriod { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Рейтинг
        /// </summary>
        public string Rating { get; set; }

        /// <summary>
        /// Номинальная стоимость
        /// </summary>
        public decimal FaceValue { get; set; }

        /// <summary>
        /// Текущая цена
        /// </summary>
        public decimal CurrentPrice { get; set; }

        /// <summary>
        /// Массив купонов
        /// </summary>
        public ICollection<Coupon> Coupons { get; set; }
    }
}

