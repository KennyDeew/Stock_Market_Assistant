using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;

namespace StockMarketAssistant.AnalyticsService.Domain.Services
{
    /// <summary>
    /// Доменный сервис для расчета рейтингов активов
    /// Содержит бизнес-логику, которая не умещается в отдельные сущности
    /// </summary>
    public class RatingCalculationService
    {
        /// <summary>
        /// Создать рейтинг актива из группы транзакций
        /// </summary>
        /// <param name="transactionGroup">Группа транзакций, сгруппированная по StockCardId</param>
        /// <param name="period">Период анализа</param>
        /// <param name="context">Контекст анализа (Global или Portfolio)</param>
        /// <param name="portfolioId">Идентификатор портфеля (обязателен для Portfolio контекста, null для Global)</param>
        /// <param name="assetType">Тип актива</param>
        /// <param name="ticker">Тикер актива</param>
        /// <param name="name">Название актива</param>
        /// <returns>Созданный рейтинг актива</returns>
        /// <exception cref="ArgumentException">Если параметры невалидны</exception>
        public AssetRating CreateRating(
            IGrouping<Guid, AssetTransaction> transactionGroup,
            Period period,
            AnalysisContext context,
            Guid? portfolioId,
            AssetType assetType,
            string ticker,
            string name)
        {
            // Валидация контекста и portfolioId
            if (context == AnalysisContext.Portfolio)
            {
                if (portfolioId == null || portfolioId == Guid.Empty)
                {
                    throw new ArgumentException(
                        "Идентификатор портфеля обязателен для контекста Portfolio",
                        nameof(portfolioId));
                }
            }
            else if (context == AnalysisContext.Global)
            {
                if (portfolioId != null)
                {
                    throw new ArgumentException(
                        "Идентификатор портфеля должен быть null для контекста Global",
                        nameof(portfolioId));
                }
            }

            // Разделение транзакций на покупки и продажи
            var buyTransactions = transactionGroup
                .Where(t => t.TransactionType == TransactionType.Buy)
                .ToList();

            var sellTransactions = transactionGroup
                .Where(t => t.TransactionType == TransactionType.Sell)
                .ToList();

            // Расчет счетчиков и сумм
            var buyCount = buyTransactions.Count;
            var sellCount = sellTransactions.Count;
            var totalBuyAmount = buyTransactions.Sum(t => t.TotalAmount);
            var totalSellAmount = sellTransactions.Sum(t => t.TotalAmount);
            var totalBuyQuantity = buyTransactions.Sum(t => t.Quantity);
            var totalSellQuantity = sellTransactions.Sum(t => t.Quantity);

            // Создание рейтинга в зависимости от контекста
            AssetRating rating;
            if (context == AnalysisContext.Portfolio)
            {
                rating = AssetRating.CreatePortfolioRating(
                    portfolioId: portfolioId!.Value,
                    stockCardId: transactionGroup.Key,
                    assetType: assetType,
                    ticker: ticker,
                    name: name,
                    periodStart: period.Start,
                    periodEnd: period.End);
            }
            else
            {
                rating = AssetRating.CreateGlobalRating(
                    stockCardId: transactionGroup.Key,
                    assetType: assetType,
                    ticker: ticker,
                    name: name,
                    periodStart: period.Start,
                    periodEnd: period.End);
            }

            // Обновление статистики
            // Временно устанавливаем ранги = 1, они будут пересчитаны через AssignRanks
            rating.UpdateStatistics(
                buyTransactionCount: buyCount,
                sellTransactionCount: sellCount,
                totalBuyAmount: totalBuyAmount,
                totalSellAmount: totalSellAmount,
                totalBuyQuantity: totalBuyQuantity,
                totalSellQuantity: totalSellQuantity,
                transactionCountRank: 1, // Временно 1, будет пересчитан через AssignRanks
                transactionAmountRank: 1  // Временно 1, будет пересчитан через AssignRanks
            );

            return rating;
        }

        /// <summary>
        /// Назначить ранги коллекции рейтингов
        /// </summary>
        /// <param name="ratings">Коллекция рейтингов для ранжирования</param>
        /// <exception cref="ArgumentNullException">Если ratings равен null</exception>
        public void AssignRanks(IEnumerable<AssetRating> ratings)
        {
            if (ratings == null)
            {
                throw new ArgumentNullException(nameof(ratings));
            }

            var ratingsList = ratings.ToList();

            if (ratingsList.Count == 0)
            {
                return;
            }

            // Ранжирование по количеству транзакций (Buy + Sell)
            var sortedByCount = ratingsList
                .OrderByDescending(r => r.BuyTransactionCount + r.SellTransactionCount)
                .ThenBy(r => r.StockCardId) // Tie-breaker для детерминированного ранжирования
                .ToList();

            for (int i = 0; i < sortedByCount.Count; i++)
            {
                // Временно устанавливаем amountRank = 1, будет обновлено ниже
                sortedByCount[i].AssignRanks(
                    countRank: i + 1,
                    amountRank: 1);
            }

            // Ранжирование по сумме транзакций (Buy + Sell)
            var sortedByAmount = ratingsList
                .OrderByDescending(r => r.TotalBuyAmount + r.TotalSellAmount)
                .ThenBy(r => r.StockCardId) // Tie-breaker для детерминированного ранжирования
                .ToList();

            for (int i = 0; i < sortedByAmount.Count; i++)
            {
                var rating = sortedByAmount[i];
                rating.AssignRanks(
                    countRank: rating.TransactionCountRank, // Сохранить существующий ранг по количеству
                    amountRank: i + 1);
            }
        }

        /// <summary>
        /// Рассчитать инкрементальное обновление рейтинга после новой транзакции
        /// Оптимизация для event-driven агрегации
        /// </summary>
        /// <param name="existingRating">Существующий рейтинг</param>
        /// <param name="newTransaction">Новая транзакция</param>
        /// <returns>Обновленный рейтинг</returns>
        /// <exception cref="ArgumentNullException">Если параметры равны null</exception>
        public AssetRating CalculateIncrementalUpdate(
            AssetRating existingRating,
            AssetTransaction newTransaction)
        {
            if (existingRating == null)
            {
                throw new ArgumentNullException(nameof(existingRating));
            }

            if (newTransaction == null)
            {
                throw new ArgumentNullException(nameof(newTransaction));
            }

            // Клонирование существующего рейтинга
            var updated = existingRating.Clone();

            // Обновление счетчиков и сумм на основе новой транзакции
            if (newTransaction.TransactionType == TransactionType.Buy)
            {
                updated.UpdateCounts(
                    buyCount: existingRating.BuyTransactionCount + 1,
                    sellCount: existingRating.SellTransactionCount,
                    buyAmount: existingRating.TotalBuyAmount + newTransaction.TotalAmount,
                    sellAmount: existingRating.TotalSellAmount);
            }
            else
            {
                updated.UpdateCounts(
                    buyCount: existingRating.BuyTransactionCount,
                    sellCount: existingRating.SellTransactionCount + 1,
                    buyAmount: existingRating.TotalBuyAmount,
                    sellAmount: existingRating.TotalSellAmount + newTransaction.TotalAmount);
            }

            updated.MarkAsUpdated();
            return updated;
        }
    }
}

