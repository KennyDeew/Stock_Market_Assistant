using StockMarketAssistant.StockCardService.Domain.Interfaces;

namespace StockMarketAssistant.StockCardService.Domain.Entities
{
    public class FinancialReportCreatedMessage : IKafkaFinancialReport<Guid>
    {
        /// <summary>
        /// Id акции
        /// </summary>
        public Guid ShareCardId { get; set; }

        /// <summary>
        /// Наименование отчета
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Период, по которому сформирован отчет (например, Q1 2025)
        /// </summary>
        public required string Period { get; set; }
    }
}
