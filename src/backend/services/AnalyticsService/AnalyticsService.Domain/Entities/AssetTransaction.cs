using StockMarketAssistant.AnalyticsService.Domain.Enums;

namespace StockMarketAssistant.AnalyticsService.Domain.Entities
{
    /// <summary>
    /// Доменная сущность транзакции с активом
    /// </summary>
    public class AssetTransaction : BaseEntity<Guid>
    {
        /// <summary>
        /// Идентификатор портфеля
        /// </summary>
        public Guid PortfolioId { get; private set; }

        /// <summary>
        /// Идентификатор актива
        /// </summary>
        public Guid StockCardId { get; private set; }

        /// <summary>
        /// Тип актива
        /// </summary>
        public AssetType AssetType { get; private set; }

        /// <summary>
        /// Тип транзакции (покупка/продажа)
        /// </summary>
        public TransactionType TransactionType { get; private set; }

        /// <summary>
        /// Количество активов
        /// </summary>
        public int Quantity { get; private set; }

        /// <summary>
        /// Цена за единицу актива
        /// </summary>
        public decimal PricePerUnit { get; private set; }

        /// <summary>
        /// Общая стоимость транзакции
        /// </summary>
        public decimal TotalAmount { get; private set; }

        /// <summary>
        /// Время транзакции
        /// </summary>
        public DateTime TransactionTime { get; private set; }

        /// <summary>
        /// Валюта транзакции
        /// </summary>
        public string Currency { get; private set; } = "RUB";

        /// <summary>
        /// Дополнительные метаданные
        /// </summary>
        public string? Metadata { get; private set; }

        // Приватный конструктор для предотвращения прямого создания
        private AssetTransaction() : base()
        {
        }

        /// <summary>
        /// Factory method для создания транзакции (соответствует требованиям)
        /// </summary>
        /// <param name="portfolioId">Идентификатор портфеля</param>
        /// <param name="stockCardId">Идентификатор актива</param>
        /// <param name="transactionType">Тип транзакции (Buy/Sell)</param>
        /// <param name="quantity">Количество активов</param>
        /// <param name="price">Цена за единицу</param>
        /// <param name="transactionDate">Дата транзакции</param>
        /// <returns>Созданная транзакция</returns>
        /// <exception cref="ArgumentException">Если параметры невалидны</exception>
        public static AssetTransaction Create(
            Guid portfolioId,
            Guid stockCardId,
            TransactionType transactionType,
            decimal quantity,
            decimal price,
            DateTime transactionDate)
        {
            ValidateTransactionParameters(portfolioId, stockCardId, quantity, price, transactionDate);

            var transaction = new AssetTransaction
            {
                Id = Guid.NewGuid(),
                PortfolioId = portfolioId,
                StockCardId = stockCardId,
                AssetType = AssetType.Share, // Значение по умолчанию, можно расширить при необходимости
                TransactionType = transactionType,
                Quantity = (int)quantity, // Приведение к int для обратной совместимости
                PricePerUnit = price,
                TotalAmount = quantity * price,
                TransactionTime = transactionDate,
                Currency = "RUB",
                Metadata = null
            };

            return transaction;
        }

        /// <summary>
        /// Factory method для создания транзакции покупки
        /// </summary>
        /// <param name="portfolioId">Идентификатор портфеля</param>
        /// <param name="stockCardId">Идентификатор актива</param>
        /// <param name="assetType">Тип актива</param>
        /// <param name="quantity">Количество активов</param>
        /// <param name="pricePerUnit">Цена за единицу</param>
        /// <param name="transactionTime">Время транзакции</param>
        /// <param name="currency">Валюта (по умолчанию RUB)</param>
        /// <param name="metadata">Дополнительные метаданные</param>
        /// <returns>Созданная транзакция покупки</returns>
        /// <exception cref="ArgumentException">Если параметры невалидны</exception>
        public static AssetTransaction CreateBuyTransaction(
            Guid portfolioId,
            Guid stockCardId,
            AssetType assetType,
            int quantity,
            decimal pricePerUnit,
            DateTime transactionTime,
            string currency = "RUB",
            string? metadata = null)
        {
            ValidateTransactionParameters(portfolioId, stockCardId, quantity, pricePerUnit, currency);

            if (transactionTime > DateTime.UtcNow.AddMinutes(1))
            {
                throw new ArgumentException("Время транзакции не может быть в будущем", nameof(transactionTime));
            }

            var transaction = new AssetTransaction
            {
                Id = Guid.NewGuid(),
                PortfolioId = portfolioId,
                StockCardId = stockCardId,
                AssetType = assetType,
                TransactionType = TransactionType.Buy,
                Quantity = quantity,
                PricePerUnit = pricePerUnit,
                TotalAmount = quantity * pricePerUnit,
                TransactionTime = transactionTime,
                Currency = currency,
                Metadata = metadata
            };

            return transaction;
        }

        /// <summary>
        /// Factory method для создания транзакции продажи
        /// </summary>
        /// <param name="portfolioId">Идентификатор портфеля</param>
        /// <param name="stockCardId">Идентификатор актива</param>
        /// <param name="assetType">Тип актива</param>
        /// <param name="quantity">Количество активов</param>
        /// <param name="pricePerUnit">Цена за единицу</param>
        /// <param name="transactionTime">Время транзакции</param>
        /// <param name="currency">Валюта (по умолчанию RUB)</param>
        /// <param name="metadata">Дополнительные метаданные</param>
        /// <returns>Созданная транзакция продажи</returns>
        /// <exception cref="ArgumentException">Если параметры невалидны</exception>
        public static AssetTransaction CreateSellTransaction(
            Guid portfolioId,
            Guid stockCardId,
            AssetType assetType,
            int quantity,
            decimal pricePerUnit,
            DateTime transactionTime,
            string currency = "RUB",
            string? metadata = null)
        {
            ValidateTransactionParameters(portfolioId, stockCardId, quantity, pricePerUnit, currency);

            if (transactionTime > DateTime.UtcNow.AddMinutes(1))
            {
                throw new ArgumentException("Время транзакции не может быть в будущем", nameof(transactionTime));
            }

            var transaction = new AssetTransaction
            {
                Id = Guid.NewGuid(),
                PortfolioId = portfolioId,
                StockCardId = stockCardId,
                AssetType = assetType,
                TransactionType = TransactionType.Sell,
                Quantity = quantity,
                PricePerUnit = pricePerUnit,
                TotalAmount = quantity * pricePerUnit,
                TransactionTime = transactionTime,
                Currency = currency,
                Metadata = metadata
            };

            return transaction;
        }

        /// <summary>
        /// Валидация параметров транзакции (базовая версия для метода Create)
        /// </summary>
        private static void ValidateTransactionParameters(
            Guid portfolioId,
            Guid stockCardId,
            decimal quantity,
            decimal price,
            DateTime transactionDate)
        {
            if (portfolioId == Guid.Empty)
            {
                throw new ArgumentException("Идентификатор портфеля не может быть пустым", nameof(portfolioId));
            }

            if (stockCardId == Guid.Empty)
            {
                throw new ArgumentException("Идентификатор актива не может быть пустым", nameof(stockCardId));
            }

            if (quantity <= 0)
            {
                throw new ArgumentException("Количество активов должно быть больше нуля", nameof(quantity));
            }

            if (price < 0)
            {
                throw new ArgumentException("Цена за единицу не может быть отрицательной", nameof(price));
            }

            if (transactionDate > DateTime.UtcNow.AddMinutes(1))
            {
                throw new ArgumentException("Дата транзакции не может быть в будущем", nameof(transactionDate));
            }
        }

        /// <summary>
        /// Валидация параметров транзакции (расширенная версия для CreateBuyTransaction/CreateSellTransaction)
        /// </summary>
        private static void ValidateTransactionParameters(
            Guid portfolioId,
            Guid stockCardId,
            int quantity,
            decimal pricePerUnit,
            string currency)
        {
            if (portfolioId == Guid.Empty)
            {
                throw new ArgumentException("Идентификатор портфеля не может быть пустым", nameof(portfolioId));
            }

            if (stockCardId == Guid.Empty)
            {
                throw new ArgumentException("Идентификатор актива не может быть пустым", nameof(stockCardId));
            }

            if (quantity <= 0)
            {
                throw new ArgumentException("Количество активов должно быть больше нуля", nameof(quantity));
            }

            if (pricePerUnit < 0)
            {
                throw new ArgumentException("Цена за единицу не может быть отрицательной", nameof(pricePerUnit));
            }

            if (string.IsNullOrWhiteSpace(currency))
            {
                throw new ArgumentException("Валюта не может быть пустой", nameof(currency));
            }

            if (currency.Length > 10)
            {
                throw new ArgumentException("Валюта не может быть длиннее 10 символов", nameof(currency));
            }
        }
    }

    /// <summary>
    /// Тип актива
    /// </summary>
    public enum AssetType
    {
        Share = 1,      // Акция
        Bond = 2,       // Облигация
        Crypto = 3      // Криптовалюта
    }

}

