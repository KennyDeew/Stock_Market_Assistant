namespace StockMarketAssistant.SharedLibrary.Events
{
    public class FinancialReportCreatedMessage
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
