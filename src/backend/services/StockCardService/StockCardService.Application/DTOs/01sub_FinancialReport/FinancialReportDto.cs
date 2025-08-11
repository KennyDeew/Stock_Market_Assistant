namespace StockMarketAssistant.StockCardService.Application.DTOs._01sub_FinancialReport
{
    /// <summary>
    /// Dto финансового отчета
    /// </summary>
    public class FinancialReportDto
    {
        /// <summary>
        /// Id финансового отчета
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Наименование финансового отчета
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание финансового отчета
        /// </summary>
        public string Description { get; set; }
    }
}
