using Microsoft.Extensions.Logging;
using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Caching;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Gateways;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Domain.Entities;
using StockMarketAssistant.PortfolioService.Domain.Enums;

namespace StockMarketAssistant.PortfolioService.Application.Services
{
    /// <summary>
    /// Сервис работы с финансовыми активами в портфеле
    /// </summary>
    public class PortfolioAssetAppService(IPortfolioAssetRepository portfolioAssetRepository, IPortfolioRepository portfolioRepository, IStockCardServiceGateway stockCardServiceGateway, ICacheService cache, ILogger<PortfolioAssetAppService> logger) : IPortfolioAssetAppService
    {
        private readonly IPortfolioAssetRepository _portfolioAssetRepository = portfolioAssetRepository;
        private readonly IPortfolioRepository _portfolioRepository = portfolioRepository;
        private readonly IStockCardServiceGateway _stockCardServiceGateway = stockCardServiceGateway;
        private readonly ICacheService _cache = cache;
        private readonly ILogger<PortfolioAssetAppService> _logger = logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Инвалидация записи кэша для финансового актива
        /// </summary>
        /// <param name="assetId">Id актива</param>
        /// <returns></returns>
        private async Task InvalidateAssetCacheAsync(Guid assetId)
        {
            await _cache.RemoveAsync($"asset_{assetId}");
        }

        private async Task<StockCardInfoDto> GetStockCardInfoAsync(PortfolioAssetType assetType, Guid stockCardId)
        {
            return assetType switch
            {
                PortfolioAssetType.Share => await GetShareCardInfoAsync(stockCardId),
                PortfolioAssetType.Bond => await GetBondCardInfoAsync(stockCardId),
                _ => new StockCardInfoDto(string.Empty, string.Empty, string.Empty)
            };
        }

        private async Task<StockCardInfoDto> GetShareCardInfoAsync(Guid stockCardId)
        {
            var shareCard = await _stockCardServiceGateway.GetShortShareCardModelByIdAsync(stockCardId);
            if (shareCard is null)
            {
                _logger.LogWarning("Акция с ID {StockCardId} не найдена в сервисе карточек активов", stockCardId);
                return new StockCardInfoDto(string.Empty, string.Empty, string.Empty);
            }
            return new StockCardInfoDto(shareCard.Ticker, shareCard.Name, shareCard.Description ?? string.Empty, shareCard.Currency);
        }

        private async Task<StockCardInfoDto> GetBondCardInfoAsync(Guid stockCardId)
        {
            var bondCard = await _stockCardServiceGateway.GetShortBondCardModelByIdAsync(stockCardId);
            if (bondCard is null)
            {
                _logger.LogWarning("Облигация с ID {StockCardId} не найдена в сервисе карточек активов", stockCardId);
                return new StockCardInfoDto(string.Empty, string.Empty, string.Empty);
            }
            return new StockCardInfoDto(bondCard.Ticker, bondCard.Name, bondCard.Description ?? string.Empty, bondCard.Currency);
        }

        /// <summary>
        /// Создать актив ценной бумаги в портфеле
        /// </summary>
        /// <param name="dto">DTO создаваемого актива</param>
        /// <returns>DTO созданного актива</returns>
        public async Task<PortfolioAssetDto> CreateAsync(CreatingPortfolioAssetDto dto)
        {
            try
            {
                // Проверка перед созданием
                if (!await _portfolioRepository.ExistsAsync(dto.PortfolioId))
                    throw new InvalidOperationException($"Портфель с ID {dto.PortfolioId} не найден");
                PortfolioAsset? existingAsset = await _portfolioAssetRepository.GetByPortfolioAndStockCardAsync(dto.PortfolioId, dto.StockCardId);
                if (existingAsset != null)
                {
                    throw new InvalidOperationException($"Актив с StockCardId {dto.StockCardId} уже существует в портфеле");
                }
                if (dto.Quantity <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(dto), "Количество должно быть больше нуля");
                }

                PortfolioAsset asset = new(Guid.NewGuid(), dto.PortfolioId, dto.StockCardId, dto.AssetType);
                // Вызов внешнего микросервиса карточки актива
                var cardInfo = await GetStockCardInfoAsync(asset.AssetType, asset.StockCardId);

                // Создаем начальную транзакцию
                var initialTransaction = new PortfolioAssetTransaction(
                    Guid.NewGuid(),
                    asset.Id,
                    PortfolioAssetTransactionType.Buy,
                    dto.Quantity,
                    dto.PurchasePricePerUnit,
                    DateTime.UtcNow,
                    cardInfo.Currency);
                asset.Transactions.Add(initialTransaction);
                PortfolioAsset createdAsset = await _portfolioAssetRepository.AddAsync(asset);

                return new PortfolioAssetDto(
                    createdAsset.Id,
                    createdAsset.PortfolioId,
                    createdAsset.StockCardId,
                    createdAsset.AssetType,
                    cardInfo.Ticker,
                    cardInfo.Name,
                    cardInfo.Description,
                    initialTransaction.Quantity,
                    initialTransaction.PricePerUnit,
                    cardInfo.Currency,
                    initialTransaction.TransactionDate,
                    []
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании актива");
                throw;
            }
        }

        /// <summary>
        /// Удалить актив из портфеля
        /// </summary>
        /// <param name="id">Идентификатор актива</param>
        /// <returns>Успешность операции</returns>
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                PortfolioAsset? asset = await _portfolioAssetRepository.GetByIdAsync(id);
                if (asset is null)
                {
                    _logger.LogWarning("Актив портфеля с ID {AssetId} не найден при попытке удаления", id);
                    return false;
                }
                await _portfolioAssetRepository.DeleteAsync(asset);
                await InvalidateAssetCacheAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при удалении актива портфеля с ID {AssetId}", id);
                throw;
            }
        }

        /// <summary>
        /// Удалить транзакцию
        /// </summary>
        /// <param name="transactionId">Идентификатор транзакции</param>
        /// <returns></returns>
        public async Task<bool> DeleteAssetTransactionAsync(Guid transactionId)
        {
            try
            {
                PortfolioAssetTransaction? transaction = await _portfolioAssetRepository.GetAssetTransactionByIdAsync(transactionId);
                if (transaction is null)
                {
                    _logger.LogWarning("Транзакция с ID {TransactionId} не найдена при попытке удаления", transactionId);
                    return false;
                }

                PortfolioAsset? asset = await _portfolioAssetRepository.GetByIdAsync(transaction.PortfolioAssetId, a => a.Transactions);
                if (asset is null)
                {
                    _logger.LogWarning("Актив для транзакции {TransactionId} не найден", transactionId);
                    return false;
                }

                await _portfolioAssetRepository.DeleteAssetTransactionAsync(transactionId);

                _logger.LogInformation("Транзакция {TransactionId} успешно удалена", transactionId);

                // Проверяем, не нужно ли удалить актив после удаления транзакции
                var remainingTransactions = await _portfolioAssetRepository.GetAssetTransactionsCountAsync(asset.Id);
                if (remainingTransactions == 0)
                {
                    await _portfolioAssetRepository.DeleteAsync(asset);
                }
                await InvalidateAssetCacheAsync(asset.Id); // данные изменились — кэш устарел
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при удалении транзакции с ID {TransactionId}", transactionId);
                throw;
            }
        }

        /// <summary>
        /// Получить актив ценной бумаги из портфеля
        /// </summary>
        /// <param name="id">Идентификатор актива</param>
        /// <returns></returns>
        public async Task<PortfolioAssetDto?> GetByIdAsync(Guid id)
        {
            string cacheKey = $"asset_{id}";

            try
            {
                var cached = await _cache.GetAsync<PortfolioAssetDto>(cacheKey);
                if (cached != null)
                    return cached;

                PortfolioAsset? asset = await _portfolioAssetRepository.GetByIdAsync(id, a => a.Transactions);
                if (asset is null)
                    return null;

                // Вызов внешнего микросервиса карточки актива
                var cardInfo = await GetStockCardInfoAsync(asset.AssetType, asset.StockCardId);
                var assetDto = new PortfolioAssetDto(
                    asset.Id,
                    asset.PortfolioId,
                    asset.StockCardId,
                    asset.AssetType,
                    cardInfo.Ticker,
                    cardInfo.Name,
                    cardInfo.Description,
                    asset.TotalQuantity,
                    asset.AveragePurchasePrice,
                    cardInfo.Currency,
                    asset.LastUpdated,
                    [.. asset.Transactions.OrderBy(t => t.TransactionDate).Select(t =>
                new PortfolioAssetTransactionDto(
                    t.Id, t.PortfolioAssetId, t.TransactionDate, t.TransactionType,
                    t.Quantity, t.PricePerUnit, t.Currency))]
                );
                
                await _cache.SetAsync(cacheKey, assetDto, _cacheExpiration);
                return assetDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении актива по ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Существует ли актив ценной бумаги
        /// </summary>
        /// <param name="id">Идентификатор актива</param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                return await _portfolioAssetRepository.ExistsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке существования актива: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Получить транзакцию
        /// </summary>
        /// <param name="transactionId">Идентификатор транзакции</param>
        /// <returns></returns>
        public async Task<PortfolioAssetTransactionDto?> GetAssetTransactionByIdAsync(Guid transactionId)
        {
            try
            {
                PortfolioAssetTransaction? transaction = await _portfolioAssetRepository.GetAssetTransactionByIdAsync(transactionId);
                return transaction is null ? null : new PortfolioAssetTransactionDto(transaction.Id, transaction.PortfolioAssetId, transaction.TransactionDate, transaction.TransactionType, transaction.Quantity, transaction.PricePerUnit, transaction.Currency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении транзакции: {TransactionId}", transactionId);
                throw;
            }
        }

        /// <summary>
        /// Получить все транзакции актива
        /// </summary>
        /// <param name="assetId">Идентификатор актива</param>
        /// <returns></returns>
        public async Task<IEnumerable<PortfolioAssetTransactionDto>> GetAssetTransactionsByAssetIdAsync(Guid assetId)
        {
            try
            {
                IEnumerable<PortfolioAssetTransaction> transactions = await _portfolioAssetRepository.GetAssetTransactionsByAssetIdAsync(assetId);
                return transactions.Select(t =>
                    new PortfolioAssetTransactionDto(t.Id, t.PortfolioAssetId, t.TransactionDate, t.TransactionType, t.Quantity, t.PricePerUnit, t.Currency));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении транзакций актива: {AssetId}", assetId);
                throw;
            }
        }

        /// <summary>
        /// Получить транзакции актива за период
        /// </summary>
        /// <param name="assetId">Идентификатор актива</param>
        /// <param name="startDate">Начальная дата периода</param>
        /// <param name="endDate">Конечная дата периода</param>
        /// <returns></returns>
        public async Task<IEnumerable<PortfolioAssetTransactionDto>> GetAssetTransactionsByAssetIdAndPeriodAsync(
            Guid assetId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var transactions = await _portfolioAssetRepository.GetAssetTransactionsByAssetIdAndPeriodAsync(assetId, startDate, endDate);
                return transactions.Select(t =>
                    new PortfolioAssetTransactionDto(t.Id, t.PortfolioAssetId, t.TransactionDate, t.TransactionType, t.Quantity, t.PricePerUnit, t.Currency));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении транзакций актива {AssetId} за период {StartDate} - {EndDate}",
                    assetId, startDate, endDate);
                throw;
            }
        }

        /// <summary>
        /// Добавить транзакцию покупки/продажи актива ценной бумаги в портфеле
        /// </summary>
        /// <param name="dto">DTO создаваемой транзакции</param>
        /// <returns>DTO созданной транзакции</returns>
        public async Task<PortfolioAssetTransactionDto> AddAssetTransactionAsync(Guid assetId, CreatingPortfolioAssetTransactionDto dto)
        {
            try
            {
                // Проверка перед созданием
                PortfolioAsset? asset = await _portfolioAssetRepository.GetByIdAsync(assetId, a => a.Transactions) ?? throw new KeyNotFoundException($"Актив с ID {assetId} не найден");

                if (dto.Quantity <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(dto), "Количество должно быть больше нуля");
                }

                // Проверяем возможность продажи
                if (dto.TransactionType == PortfolioAssetTransactionType.Sell && dto.Quantity > asset.TotalQuantity)
                {
                    throw new InvalidOperationException("Недостаточно активов для продажи");
                }
                // Вызов внешнего микросервиса карточки актива
                var cardInfo = await GetStockCardInfoAsync(asset.AssetType, asset.StockCardId);

                PortfolioAssetTransaction transaction = new(
                    Guid.NewGuid(),
                    assetId,
                    dto.TransactionType,
                    dto.Quantity,
                    dto.PricePerUnit,
                    dto.TransactionDate ?? DateTime.UtcNow,
                    dto.Currency ?? cardInfo.Currency);

                PortfolioAssetTransaction createdAssetTransaction = await _portfolioAssetRepository.AddAssetTransactionAsync(transaction);
                asset.Transactions.Add(transaction);

                // Проверяем, нужно ли удалять актив (количество стало 0)
                if (asset.TotalQuantity == 0)
                {
                    await _portfolioAssetRepository.DeleteAsync(asset);
                }
                await InvalidateAssetCacheAsync(assetId); // данные изменились — кэш устарел

                return new PortfolioAssetTransactionDto(createdAssetTransaction.Id, createdAssetTransaction.PortfolioAssetId, createdAssetTransaction.TransactionDate, createdAssetTransaction.TransactionType, createdAssetTransaction.Quantity, createdAssetTransaction.PricePerUnit, createdAssetTransaction.Currency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании актива");
                throw;
            }
        }

        public async Task UpdateAssetTransactionAsync(
            Guid transactionId,
            UpdatingPortfolioAssetTransactionDto updatingTransactionDto)
        {
            _logger.LogInformation("Обновление транзакции {TransactionId}", transactionId);

            try
            {
                // Валидация входных данных
                if (updatingTransactionDto.Quantity <= 0)
                {
                    _logger.LogWarning("Попытка обновления транзакции с некорректным количеством: {Quantity}",
                                     updatingTransactionDto.Quantity);
                    throw new ArgumentException("Количество должно быть больше нуля");
                }

                if (updatingTransactionDto.PricePerUnit < 0)
                {
                    _logger.LogWarning("Попытка обновления транзакции с отрицательной ценой: {Price}",
                                     updatingTransactionDto.PricePerUnit);
                    throw new ArgumentException("Цена не может быть отрицательной");
                }

                PortfolioAssetTransaction? transaction = await _portfolioAssetRepository.GetAssetTransactionByIdAsync(transactionId);
                if (transaction is null)
                {
                    _logger.LogWarning("Транзакция {TransactionId} не найдена", transactionId);
                    return;
                }

                var asset = await _portfolioAssetRepository.GetByIdAsync(transaction.PortfolioAssetId);
                if (asset == null)
                {
                    _logger.LogError("Актив не найден для транзакции {TransactionId}", transactionId);
                    throw new InvalidOperationException("Актив портфеля не найден");
                }

                // Обновляем транзакцию
                transaction.TransactionType = updatingTransactionDto.TransactionType;
                transaction.Quantity = updatingTransactionDto.Quantity;
                transaction.PricePerUnit = updatingTransactionDto.PricePerUnit;
                transaction.TransactionDate = updatingTransactionDto.TransactionDate;
                transaction.Currency = updatingTransactionDto.Currency;

                // Сохраняем изменения
                await _portfolioAssetRepository.UpdateAssetTransactionAsync(transaction);
                await InvalidateAssetCacheAsync(transaction.PortfolioAssetId); // данные изменились — кэш устарел

                _logger.LogInformation("Транзакция {TransactionId} успешно обновлена", transactionId);
            }
            catch (ArgumentException)
            {
                // Пробрасываем валидационные исключения
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении транзакции {TransactionId}", transactionId);
                throw;
            }
        }
    }
}
