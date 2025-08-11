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
        /// Купоны облигации
        /// </summary>
        public List<CouponModel>? Coupons { get; set; }
    }
}
