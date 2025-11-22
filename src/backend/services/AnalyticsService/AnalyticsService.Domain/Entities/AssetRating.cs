namespace StockMarketAssistant.AnalyticsService.Domain.Entities
{
    /// <summary>
    /// Доменная сущность рейтинга актива
    /// </summary>
    public class AssetRating : BaseEntity<Guid>
    {
        /// <summary>
        /// Идентификатор актива
        /// </summary>
        public Guid StockCardId { get; private set; }

        /// <summary>
        /// Тип актива
        /// </summary>
        public AssetType AssetType { get; private set; }

        /// <summary>
        /// Тикер актива
        /// </summary>
        public string Ticker { get; private set; } = string.Empty;

        /// <summary>
        /// Название актива
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// Период анализа (начало)
        /// </summary>
        public DateTime PeriodStart { get; private set; }

        /// <summary>
        /// Период анализа (конец)
        /// </summary>
        public DateTime PeriodEnd { get; private set; }

        /// <summary>
        /// Количество транзакций покупки
        /// </summary>
        public int BuyTransactionCount { get; private set; }

        /// <summary>
        /// Количество транзакций продажи
        /// </summary>
        public int SellTransactionCount { get; private set; }

        /// <summary>
        /// Общая стоимость покупок
        /// </summary>
        public decimal TotalBuyAmount { get; private set; }

        /// <summary>
        /// Общая стоимость продаж
        /// </summary>
        public decimal TotalSellAmount { get; private set; }

        /// <summary>
        /// Общее количество купленных активов
        /// </summary>
        public int TotalBuyQuantity { get; private set; }

        /// <summary>
        /// Общее количество проданных активов
        /// </summary>
        public int TotalSellQuantity { get; private set; }

        /// <summary>
        /// Рейтинг по количеству транзакций
        /// </summary>
        public int TransactionCountRank { get; private set; }

        /// <summary>
        /// Рейтинг по стоимости транзакций
        /// </summary>
        public int TransactionAmountRank { get; private set; }

        /// <summary>
        /// Время последнего обновления
        /// </summary>
        public DateTime LastUpdated { get; private set; }

        /// <summary>
        /// Контекст анализа (портфель или глобальный)
        /// </summary>
        public AnalysisContext Context { get; private set; }

        /// <summary>
        /// Идентификатор портфеля (если анализ в контексте портфеля)
        /// </summary>
        public Guid? PortfolioId { get; private set; }

        // Приватный конструктор для предотвращения прямого создания
        private AssetRating() : base()
        {
        }

        /// <summary>
        /// Factory method для создания рейтинга актива в контексте портфеля
        /// </summary>
        /// <param name="portfolioId">Идентификатор портфеля</param>
        /// <param name="stockCardId">Идентификатор актива</param>
        /// <param name="assetType">Тип актива</param>
        /// <param name="ticker">Тикер актива</param>
        /// <param name="name">Название актива</param>
        /// <param name="periodStart">Начало периода анализа</param>
        /// <param name="periodEnd">Конец периода анализа</param>
        /// <returns>Созданный рейтинг актива</returns>
        /// <exception cref="ArgumentException">Если параметры невалидны</exception>
        public static AssetRating CreatePortfolioRating(
            Guid portfolioId,
            Guid stockCardId,
            AssetType assetType,
            string ticker,
            string name,
            DateTime periodStart,
            DateTime periodEnd)
        {
            ValidateRatingParameters(portfolioId, stockCardId, ticker, name, periodStart, periodEnd, AnalysisContext.Portfolio);

            var rating = new AssetRating
            {
                Id = Guid.NewGuid(),
                PortfolioId = portfolioId,
                StockCardId = stockCardId,
                AssetType = assetType,
                Ticker = ticker,
                Name = name,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                Context = AnalysisContext.Portfolio,
                LastUpdated = DateTime.UtcNow
            };

            return rating;
        }

        /// <summary>
        /// Factory method для создания глобального рейтинга актива
        /// </summary>
        /// <param name="stockCardId">Идентификатор актива</param>
        /// <param name="assetType">Тип актива</param>
        /// <param name="ticker">Тикер актива</param>
        /// <param name="name">Название актива</param>
        /// <param name="periodStart">Начало периода анализа</param>
        /// <param name="periodEnd">Конец периода анализа</param>
        /// <returns>Созданный рейтинг актива</returns>
        /// <exception cref="ArgumentException">Если параметры невалидны</exception>
        public static AssetRating CreateGlobalRating(
            Guid stockCardId,
            AssetType assetType,
            string ticker,
            string name,
            DateTime periodStart,
            DateTime periodEnd)
        {
            ValidateRatingParameters(Guid.Empty, stockCardId, ticker, name, periodStart, periodEnd, AnalysisContext.Global);

            var rating = new AssetRating
            {
                Id = Guid.NewGuid(),
                PortfolioId = null,
                StockCardId = stockCardId,
                AssetType = assetType,
                Ticker = ticker,
                Name = name,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                Context = AnalysisContext.Global,
                LastUpdated = DateTime.UtcNow
            };

            return rating;
        }

        /// <summary>
        /// Обновить счётчики транзакций и суммы (соответствует требованиям)
        /// </summary>
        /// <param name="buyCount">Количество транзакций покупки</param>
        /// <param name="sellCount">Количество транзакций продажи</param>
        /// <param name="buyAmount">Общая стоимость покупок</param>
        /// <param name="sellAmount">Общая стоимость продаж</param>
        /// <exception cref="ArgumentException">Если параметры невалидны</exception>
        public void UpdateCounts(int buyCount, int sellCount, decimal buyAmount, decimal sellAmount)
        {
            if (buyCount < 0)
            {
                throw new ArgumentException("Количество транзакций покупки не может быть отрицательным", nameof(buyCount));
            }

            if (sellCount < 0)
            {
                throw new ArgumentException("Количество транзакций продажи не может быть отрицательным", nameof(sellCount));
            }

            if (buyAmount < 0)
            {
                throw new ArgumentException("Общая стоимость покупок не может быть отрицательной", nameof(buyAmount));
            }

            if (sellAmount < 0)
            {
                throw new ArgumentException("Общая стоимость продаж не может быть отрицательной", nameof(sellAmount));
            }

            BuyTransactionCount = buyCount;
            SellTransactionCount = sellCount;
            TotalBuyAmount = buyAmount;
            TotalSellAmount = sellAmount;
            MarkAsUpdated();
        }

        /// <summary>
        /// Назначить ранги (соответствует требованиям)
        /// </summary>
        /// <param name="countRank">Рейтинг по количеству транзакций</param>
        /// <param name="amountRank">Рейтинг по стоимости транзакций</param>
        /// <exception cref="ArgumentException">Если параметры невалидны</exception>
        public void AssignRanks(int countRank, int amountRank)
        {
            if (countRank <= 0)
            {
                throw new ArgumentException("Рейтинг по количеству транзакций должен быть больше нуля", nameof(countRank));
            }

            if (amountRank <= 0)
            {
                throw new ArgumentException("Рейтинг по стоимости транзакций должен быть больше нуля", nameof(amountRank));
            }

            TransactionCountRank = countRank;
            TransactionAmountRank = amountRank;
            MarkAsUpdated();
        }

        /// <summary>
        /// Отметить как обновлённый (соответствует требованиям)
        /// </summary>
        public void MarkAsUpdated()
        {
            LastUpdated = DateTime.UtcNow;
            UpdateTimestamp();
        }

        /// <summary>
        /// Создать копию рейтинга для инкрементального обновления (соответствует требованиям)
        /// </summary>
        /// <returns>Новая копия рейтинга</returns>
        public AssetRating Clone()
        {
            var clone = new AssetRating
            {
                Id = Guid.NewGuid(),
                StockCardId = StockCardId,
                AssetType = AssetType,
                Ticker = Ticker,
                Name = Name,
                PeriodStart = PeriodStart,
                PeriodEnd = PeriodEnd,
                BuyTransactionCount = BuyTransactionCount,
                SellTransactionCount = SellTransactionCount,
                TotalBuyAmount = TotalBuyAmount,
                TotalSellAmount = TotalSellAmount,
                TotalBuyQuantity = TotalBuyQuantity,
                TotalSellQuantity = TotalSellQuantity,
                TransactionCountRank = TransactionCountRank,
                TransactionAmountRank = TransactionAmountRank,
                LastUpdated = LastUpdated,
                Context = Context,
                PortfolioId = PortfolioId
            };

            return clone;
        }

        /// <summary>
        /// Обновить статистику рейтинга
        /// </summary>
        /// <param name="buyTransactionCount">Количество транзакций покупки</param>
        /// <param name="sellTransactionCount">Количество транзакций продажи</param>
        /// <param name="totalBuyAmount">Общая стоимость покупок</param>
        /// <param name="totalSellAmount">Общая стоимость продаж</param>
        /// <param name="totalBuyQuantity">Общее количество купленных активов</param>
        /// <param name="totalSellQuantity">Общее количество проданных активов</param>
        /// <param name="transactionCountRank">Рейтинг по количеству транзакций</param>
        /// <param name="transactionAmountRank">Рейтинг по стоимости транзакций</param>
        /// <exception cref="ArgumentException">Если параметры невалидны</exception>
        public void UpdateStatistics(
            int buyTransactionCount,
            int sellTransactionCount,
            decimal totalBuyAmount,
            decimal totalSellAmount,
            int totalBuyQuantity,
            int totalSellQuantity,
            int transactionCountRank,
            int transactionAmountRank)
        {
            if (buyTransactionCount < 0)
            {
                throw new ArgumentException("Количество транзакций покупки не может быть отрицательным", nameof(buyTransactionCount));
            }

            if (sellTransactionCount < 0)
            {
                throw new ArgumentException("Количество транзакций продажи не может быть отрицательным", nameof(sellTransactionCount));
            }

            if (totalBuyAmount < 0)
            {
                throw new ArgumentException("Общая стоимость покупок не может быть отрицательной", nameof(totalBuyAmount));
            }

            if (totalSellAmount < 0)
            {
                throw new ArgumentException("Общая стоимость продаж не может быть отрицательной", nameof(totalSellAmount));
            }

            if (totalBuyQuantity < 0)
            {
                throw new ArgumentException("Общее количество купленных активов не может быть отрицательным", nameof(totalBuyQuantity));
            }

            if (totalSellQuantity < 0)
            {
                throw new ArgumentException("Общее количество проданных активов не может быть отрицательным", nameof(totalSellQuantity));
            }

            if (transactionCountRank <= 0)
            {
                throw new ArgumentException("Рейтинг по количеству транзакций должен быть больше нуля", nameof(transactionCountRank));
            }

            if (transactionAmountRank <= 0)
            {
                throw new ArgumentException("Рейтинг по стоимости транзакций должен быть больше нуля", nameof(transactionAmountRank));
            }

            BuyTransactionCount = buyTransactionCount;
            SellTransactionCount = sellTransactionCount;
            TotalBuyAmount = totalBuyAmount;
            TotalSellAmount = totalSellAmount;
            TotalBuyQuantity = totalBuyQuantity;
            TotalSellQuantity = totalSellQuantity;
            TransactionCountRank = transactionCountRank;
            TransactionAmountRank = transactionAmountRank;
            LastUpdated = DateTime.UtcNow;
            UpdateTimestamp();
        }

        /// <summary>
        /// Валидация параметров рейтинга
        /// </summary>
        private static void ValidateRatingParameters(
            Guid portfolioId,
            Guid stockCardId,
            string ticker,
            string name,
            DateTime periodStart,
            DateTime periodEnd,
            AnalysisContext context)
        {
            // Валидация PortfolioId в зависимости от контекста
            if (context == AnalysisContext.Portfolio)
            {
                if (portfolioId == Guid.Empty)
                {
                    throw new ArgumentException("Идентификатор портфеля обязателен для рейтинга портфеля", nameof(portfolioId));
                }
            }
            else if (context == AnalysisContext.Global)
            {
                if (portfolioId != Guid.Empty)
                {
                    throw new ArgumentException("Идентификатор портфеля должен быть null для глобального рейтинга", nameof(portfolioId));
                }
            }

            if (stockCardId == Guid.Empty)
            {
                throw new ArgumentException("Идентификатор актива не может быть пустым", nameof(stockCardId));
            }

            if (string.IsNullOrWhiteSpace(ticker))
            {
                throw new ArgumentException("Тикер актива не может быть пустым", nameof(ticker));
            }

            if (ticker.Length > 20)
            {
                throw new ArgumentException("Тикер актива не может быть длиннее 20 символов", nameof(ticker));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Название актива не может быть пустым", nameof(name));
            }

            if (name.Length > 255)
            {
                throw new ArgumentException("Название актива не может быть длиннее 255 символов", nameof(name));
            }

            if (periodStart >= periodEnd)
            {
                throw new ArgumentException("Начало периода должно быть раньше конца периода", nameof(periodStart));
            }

            if (periodEnd > DateTime.UtcNow.AddDays(1))
            {
                throw new ArgumentException("Конец периода не может быть в будущем", nameof(periodEnd));
            }
        }
    }

    /// <summary>
    /// Контекст анализа
    /// </summary>
    public enum AnalysisContext
    {
        Portfolio = 1,  // Анализ в контексте портфеля
        Global = 2      // Глобальный анализ по всем портфелям
    }
}

