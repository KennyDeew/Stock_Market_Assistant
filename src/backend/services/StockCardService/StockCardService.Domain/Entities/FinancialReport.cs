using StockCardService.Domain.Entities;

namespace StockMarketAssistant.StockCardService.Domain.Entities
{
    public class FinancialReport : IEntityWithParent<Guid>, IEntity<Guid>
    {
        /// <summary>
        /// Id отчета
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Id акции
        /// </summary>
        public Guid ParentId { get; set; }

        /// <summary>
        /// Наименование отчета
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание отчета
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Период, по которому сформирован отчет
        /// </summary>
        public string Period { get; set; }

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
        /// CAPEX
        /// </summary>
        public decimal CAPEX { get; set; }

        /// <summary>
        /// Свободный денежный поток
        /// </summary>
        public decimal FCF { get; set; }

        /// <summary>
        /// чистый долг
        /// </summary>
        public decimal Debt { get; set; }
    }
}

