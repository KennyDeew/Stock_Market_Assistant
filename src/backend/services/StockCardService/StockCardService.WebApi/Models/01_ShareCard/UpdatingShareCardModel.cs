namespace StockCardService.WebApi.Models.ShareCard
{
    /// <summary>
    /// Модель создаваемой карточки акции
    /// </summary>
    public class UpdatingShareCardModel
    {
        /// <summary>
        /// Id акции
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Тикер акции
        /// </summary>
        public string Ticker { get; set; }

        /// <summary>
        /// Наименование акции
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание акции
        /// </summary>
        public string Description { get; set; }
    }
}
