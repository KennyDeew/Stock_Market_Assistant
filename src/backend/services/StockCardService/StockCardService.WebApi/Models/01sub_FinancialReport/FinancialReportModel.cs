namespace StockCardService.WebApi.Models._01sub_FinancialReport
{
    /// <summary>
    /// Модель финансового отчета
    /// </summary>
    public class FinancialReportModel
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
