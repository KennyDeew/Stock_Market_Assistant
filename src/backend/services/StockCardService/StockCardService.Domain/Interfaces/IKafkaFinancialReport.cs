namespace StockMarketAssistant.StockCardService.Domain.Interfaces
{
    /// <summary>
    /// Интерфейс сообщения в Kafka по финансовому отчету
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    public interface IKafkaFinancialReport<TId>
    {
        /// <summary>
        /// Идентификатор акции, к которой относится финансовый отчет.
        /// </summary>
        TId ShareCardId { get; set; }
    }
}
