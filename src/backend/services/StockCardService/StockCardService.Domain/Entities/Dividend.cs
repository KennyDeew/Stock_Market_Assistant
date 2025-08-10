using StockCardService.Domain.Entities;

namespace StockMarketAssistant.StockCardService.Domain.Entities
{
    /// <summary>
    /// Класс дивидендов
    /// </summary>
    public class Dividend : IEntity<Guid>
    {
        /// <summary>
        /// Id дивидендов
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Id акции
        /// </summary>
        public Guid ShareCardId { get; set; }

        /// <summary>
        /// дата див отсечки
        /// </summary>
        public DateTime CuttOffDate { get; set; }

        /// <summary>
        /// период выплаты
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Размер дивидендов
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Акция
        /// </summary>
        public ShareCard ShareCard { get; set; }
    }
}

