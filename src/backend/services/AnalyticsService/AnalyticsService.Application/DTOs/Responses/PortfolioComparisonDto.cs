using StockMarketAssistant.AnalyticsService.Application.DTOs;

namespace StockMarketAssistant.AnalyticsService.Application.DTOs.Responses
{
    /// <summary>
    /// DTO ответа для сравнения портфелей
    /// </summary>
    public class PortfolioComparisonDto
    {
        /// <summary>
        /// Состояния портфелей (ключ - PortfolioId)
        /// </summary>
        public Dictionary<Guid, PortfolioStateDto> PortfolioStates { get; set; } = new();

        /// <summary>
        /// Транзакции портфелей (ключ - PortfolioId)
        /// </summary>
        public Dictionary<Guid, List<PortfolioTransactionDto>> PortfolioTransactions { get; set; } = new();

        /// <summary>
        /// Начальная дата периода (если указана)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Конечная дата периода (если указана)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Создать из результата ComparePortfoliosUseCase
        /// </summary>
        public static PortfolioComparisonDto FromComparisonResult(
            UseCases.ComparePortfoliosUseCase.ComparisonResult result,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            return new PortfolioComparisonDto
            {
                PortfolioStates = result.PortfolioStates,
                PortfolioTransactions = result.PortfolioTransactions,
                StartDate = startDate,
                EndDate = endDate
            };
        }
    }
}

