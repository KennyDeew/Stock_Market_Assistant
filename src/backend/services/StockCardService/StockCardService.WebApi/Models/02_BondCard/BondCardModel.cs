using StockMarketAssistant.StockCardService.WebApi.Models._02sub_Coupon;

namespace StockCardService.WebApi.Models.BondCard
{
    /// <summary>
    /// Модель карточки облигации
    /// </summary>
    public class BondCardModel
    {
        /// <summary>
        /// Id облигации
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Тикер облигации
        /// </summary>
        public required string Ticker { get; set; }

        /// <summary>
        /// Наименование облигации
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Описание облигации
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        public required string Currency { get; set; }

        /// <summary>
        /// дата погашения облигации
        /// </summary>
        public DateTime MaturityPeriod { get; set; }

        /// <summary>
        /// Рейтинг
        /// </summary>
        public required string Rating { get; set; }

        /// <summary>
        /// Номинальная стоимость
        /// </summary>
        public decimal FaceValue { get; set; }

        /// <summary>
        /// Текущая цена
        /// </summary>
        public decimal CurrentPrice { get; set; }

        /// <summary>
        /// Купоны облигации
        /// </summary>
        public List<CouponModel> Coupons { get; set; } = new List<CouponModel>();
    }
}
