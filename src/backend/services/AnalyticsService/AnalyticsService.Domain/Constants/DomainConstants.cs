namespace StockMarketAssistant.AnalyticsService.Domain.Constants
{
    /// <summary>
    /// Доменные константы, используемые в аналитическом сервисе
    /// </summary>
    public static class DomainConstants
    {
        /// <summary>
        /// Константы для агрегации данных
        /// </summary>
        public static class Aggregation
        {
            /// <summary>
            /// Максимальное количество топ активов в рейтинге
            /// </summary>
            public const int MaxTopAssetsCount = 100;

            /// <summary>
            /// Количество топ активов по умолчанию
            /// </summary>
            public const int DefaultTopAssetsCount = 10;

            /// <summary>
            /// Максимальная длительность периода в днях
            /// </summary>
            public const int MaxPeriodDays = 365;
        }

        /// <summary>
        /// Константы для валидации данных
        /// </summary>
        public static class Validation
        {
            /// <summary>
            /// Минимальное количество актива (поддержка криптовалют с дробными значениями)
            /// </summary>
            public const decimal MinQuantity = 0.00000001m;

            /// <summary>
            /// Минимальная цена актива
            /// </summary>
            public const decimal MinPrice = 0m;

            /// <summary>
            /// Максимальное количество портфелей в сравнении
            /// </summary>
            public const int MaxPortfoliosInComparison = 10;
        }
    }
}

