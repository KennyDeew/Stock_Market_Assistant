namespace StockCardService.WebApi.Models._01sub_FinancialReport
{
    /// <summary>
    /// Модель создаваемого финансового отчета
    /// </summary>
    public class CreatingFinancialReportModel
    {
        /// <summary>
        /// Id акции
        /// </summary>
        public Guid ParentId { get; set; }

        /// <summary>
        /// Наименование финансового отчета
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Описание финансового отчета
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Период, по которому сформирован отчет (например, Q1 2025)
        /// </summary>
        public required string Period { get; set; }

        /// <summary>
        /// Выручка
        /// </summary>
        public decimal Revenue { get; set; }

        /// <summary>
        /// EBITDA
        /// </summary>
        public decimal EBITDA { get; set; }

        /// <summary>
        /// Чистая прибыль
        /// </summary>
        public decimal NetProfit { get; set; }

        /// <summary>
        /// CAPEX (капитальные затраты)
        /// </summary>
        public decimal CAPEX { get; set; }

        /// <summary>
        /// Свободный денежный поток (Free Cash Flow)
        /// </summary>
        public decimal FCF { get; set; }

        /// <summary>
        /// чистый долг (Net Debt)
        /// </summary>
        public decimal Debt { get; set; }

        // ------------------ Баланс (отчет о финансовом положении) ------------------

        /// <summary>
        /// Общие активы (Total Assets)
        /// </summary>
        public decimal TotalAssets { get; set; }

        /// <summary>
        /// Внеоборотные активы (Non-current Assets)
        /// </summary>
        public decimal NonCurrentAssets { get; set; }

        /// <summary>
        /// Оборотные активы (Current Assets)
        /// </summary>
        public decimal CurrentAssets { get; set; }

        /// <summary>
        /// Запасы (Inventories)
        /// </summary>
        public decimal Inventories { get; set; }

        /// <summary>
        /// Дебиторская задолженность (Accounts Receivable)
        /// </summary>
        public decimal AccountsReceivable { get; set; }

        /// <summary>
        /// Денежные средства и эквиваленты (Cash Equivalents)
        /// </summary>
        public decimal CashAndEquivalents { get; set; }

        /// <summary>
        /// Долгосрочные обязательства (Non-current Liabilities)
        /// </summary>
        public decimal NonCurrentLiabilities { get; set; }

        /// <summary>
        /// Краткосрочные обязательства (Current Liabilities)
        /// </summary>
        public decimal CurrentLiabilities { get; set; }
    }
}
