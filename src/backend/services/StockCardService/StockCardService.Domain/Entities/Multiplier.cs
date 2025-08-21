using StockCardService.Domain.Entities;

namespace StockMarketAssistant.StockCardService.Domain.Entities

{
    public class Multiplier : IEntityWithParent<Guid>, IEntity<Guid>
    {
        /// <summary>
        /// Id мультипликатора
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Id акции
        /// </summary>
        public Guid ParentId { get; set; }

        /// <summary>
        /// Наименование мультипликатора
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Значение мультипликатора
        /// </summary>
        public decimal Value { get; set; }
        
        public ShareCard ShareCard { get; set; }
    }
}

