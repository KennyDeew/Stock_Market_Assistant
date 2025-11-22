namespace StockMarketAssistant.AnalyticsService.Application.DTOs.Responses
{
    /// <summary>
    /// DTO ответа для топ активов
    /// </summary>
    public class TopAssetsResponseDto
    {
        /// <summary>
        /// Период анализа
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Период анализа
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Контекст анализа
        /// </summary>
        public int Context { get; set; }

        /// <summary>
        /// Идентификатор портфеля (для Portfolio контекста)
        /// </summary>
        public Guid? PortfolioId { get; set; }

        /// <summary>
        /// Количество запрошенных топ активов
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// Список рейтингов активов
        /// </summary>
        public List<AssetRatingDto> Assets { get; set; } = new();

        /// <summary>
        /// Создать из списка доменных сущностей
        /// </summary>
        public static TopAssetsResponseDto FromEntities(
            IEnumerable<Domain.Entities.AssetRating> ratings,
            DateTime startDate,
            DateTime endDate,
            Domain.Enums.AnalysisContext context,
            Guid? portfolioId,
            int top)
        {
            return new TopAssetsResponseDto
            {
                StartDate = startDate,
                EndDate = endDate,
                Context = (int)context,
                PortfolioId = portfolioId,
                Top = top,
                Assets = ratings.Select(AssetRatingDto.FromEntity).ToList()
            };
        }
    }
}

