namespace StockCardService.WebApi.Models.BondCard
{
    /// <summary>
    /// Модель создаваемой карточки облигации
    /// </summary>
    public class UpdatingBondCardModel
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
    }
}
