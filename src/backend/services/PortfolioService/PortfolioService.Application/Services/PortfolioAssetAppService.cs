using Microsoft.Extensions.Logging;
using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Caching;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Gateways;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Security;
using StockMarketAssistant.PortfolioService.Domain.Entities;
using StockMarketAssistant.PortfolioService.Domain.Enums;
using StockMarketAssistant.PortfolioService.Domain.Exceptions;
using StockMarketAssistant.SharedLibrary.Enums;

namespace StockMarketAssistant.PortfolioService.Application.Services
{
    /// <summary>
    /// Сервис работы с финансовыми активами в портфеле
    /// </summary>
    public class PortfolioAssetAppService(IPortfolioAssetRepository portfolioAssetRepository, IUserContext userContext, IPortfolioRepository portfolioRepository, IStockCardServiceGateway stockCardServiceGateway, ICacheService cache, ILogger<PortfolioAssetAppService> logger) : IPortfolioAssetAppService
    {
        private readonly IUserContext _userContext = userContext;
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

        /// <summary>
        /// Инвалидация записи кэша для портфеля ценных бумаг
        /// </summary>
        /// <param name="portfolioId">Id портфеля</param>
        /// <returns></returns>
        private async Task InvalidatePortfolioCacheAsync(Guid portfolioId)
        {
            await _cache.RemoveAsync($"portfolio_{portfolioId}");
        }

        /// <summary>
        /// Получить информацию о ценной бумаге актива из внешнего сервиса StockCardService
        /// </summary>
        /// <param name="assetType">Тип финансового актива</param>
        /// <param name="stockCardId">Идентификатор ценной бумаги</param>
        /// <param name="toRetrieveCurrentPrice">Требуется ли считывать текущую цену по ценной бумаге</param>
        /// <returns></returns>
        public async Task<StockCardInfoDto> GetStockCardInfoAsync(PortfolioAssetType assetType, Guid stockCardId, bool toRetrieveCurrentPrice)
        {
            return assetType switch
            {
                PortfolioAssetType.Share => await GetShareCardInfoAsync(stockCardId, toRetrieveCurrentPrice),
                PortfolioAssetType.Bond => await GetBondCardInfoAsync(stockCardId, toRetrieveCurrentPrice),
                _ => new StockCardInfoDto(string.Empty, string.Empty, string.Empty)
            };
        }

        private async Task<StockCardInfoDto> GetShareCardInfoAsync(Guid stockCardId, bool toRetrieveCurrentPrice)
        {
            if (toRetrieveCurrentPrice)
                await _stockCardServiceGateway.UpdateAllPricesForShareCardsAsync();
            var shareCard = await _stockCardServiceGateway.GetShortShareCardModelByIdAsync(stockCardId);
            if (shareCard is null)
            {
                _logger.LogWarning("Акция с ID {StockCardId} не найдена в сервисе карточек активов", stockCardId);
                return new StockCardInfoDto(string.Empty, string.Empty, string.Empty);
            }
            return new StockCardInfoDto(shareCard.Ticker, shareCard.Name, shareCard.Description ?? string.Empty, toRetrieveCurrentPrice ? shareCard.CurrentPrice : null, shareCard.Currency);
        }

        private async Task<StockCardInfoDto> GetBondCardInfoAsync(Guid stockCardId, bool toRetrieveCurrentPrice)
        {
            if (toRetrieveCurrentPrice)
                await _stockCardServiceGateway.UpdateAllPricesForBondCardsAsync();
            var bondCard = await _stockCardServiceGateway.GetShortBondCardModelByIdAsync(stockCardId);
            if (bondCard is null)
            {
                _logger.LogWarning("Облигация с ID {StockCardId} не найдена в сервисе карточек активов", stockCardId);
                return new StockCardInfoDto(string.Empty, string.Empty, string.Empty);
            }
            return new StockCardInfoDto(bondCard.Ticker, bondCard.Name, bondCard.Description ?? string.Empty, null, bondCard.Currency);
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
                // Проверка существования портфеля
                Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(dto.PortfolioId);
                if (portfolio == null)
                {
                    _logger.LogWarning("Портфель с ID {PortfolioId} не найден при создании актива", dto.PortfolioId);
                    throw new KeyNotFoundException($"Портфель с ID {dto.PortfolioId} не найден");
                }

                // Проверка прав доступа: USER может создавать активы только в своих портфелях
                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} пытается создать актив в чужом портфеле {PortfolioId}",
                        _userContext.UserId, dto.PortfolioId);
                    throw new KeyNotFoundException($"Портфель с ID {dto.PortfolioId} не найден");
                }

                // Проверка дубликата
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
                var cardInfo = await GetStockCardInfoAsync(asset.AssetType, asset.StockCardId, false);

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
                await InvalidatePortfolioCacheAsync(dto.PortfolioId);

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

                Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(asset.PortfolioId);
                if (portfolio == null)
                {
                    _logger.LogWarning("Портфель {PortfolioId} для актива {AssetId} не найден", asset.PortfolioId, id);
                    throw new KeyNotFoundException("Портфель не найден");
                }

                // Проверка прав доступа
                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} пытается удалить актив {AssetId} из чужого портфеля {PortfolioId}",
                        _userContext.UserId, id, asset.PortfolioId);
                    throw new SecurityException("Доступ запрещён");
                }

                await _portfolioAssetRepository.DeleteAsync(asset);
                await InvalidateAssetCacheAsync(id);
                await InvalidatePortfolioCacheAsync(portfolio.Id);

                _logger.LogInformation("Актив {AssetId} успешно удалён из портфеля {PortfolioId}", id, portfolio.Id);
                return true;
            }
            catch (SecurityException)
            {
                throw;
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
                    _logger.LogWarning("Транзакция с ID {TransactionId} не найдена", transactionId);
                    return false;
                }

                PortfolioAsset? asset = await _portfolioAssetRepository.GetByIdAsync(transaction.PortfolioAssetId);
                if (asset == null)
                {
                    _logger.LogWarning("Актив для транзакции {TransactionId} не найден", transactionId);
                    return false;
                }

                Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(asset.PortfolioId);
                if (portfolio == null)
                {
                    _logger.LogWarning("Портфель {PortfolioId} для актива {AssetId} не найден", asset.PortfolioId, asset.Id);
                    throw new KeyNotFoundException("Портфель не найден");
                }

                // Проверка прав доступа
                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} пытается удалить транзакцию {TransactionId} из чужого портфеля",
                        _userContext.UserId, transactionId);
                    throw new SecurityException("Доступ запрещён");
                }

                await _portfolioAssetRepository.DeleteAssetTransactionAsync(transactionId);

                // Удаление актива, если транзакций не осталось
                var remainingTransactions = await _portfolioAssetRepository.GetAssetTransactionsCountAsync(asset.Id);
                if (remainingTransactions == 0)
                {
                    await _portfolioAssetRepository.DeleteAsync(asset);
                    await InvalidatePortfolioCacheAsync(portfolio.Id);
                }

                await InvalidateAssetCacheAsync(asset.Id);
                _logger.LogInformation("Транзакция {TransactionId} успешно удалена", transactionId);
                return true;
            }
            catch (SecurityException)
            {
                throw;
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
                {
                    _logger.LogWarning("Актив портфеля с ID {AssetId} не найден", id);
                    return null;
                }

                Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(asset.PortfolioId);
                if (portfolio == null)
                {
                    _logger.LogWarning("Портфель {PortfolioId} для актива {AssetId} не найден", asset.PortfolioId, id);
                    return null;
                }

                // Проверка прав доступа
                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} пытается получить доступ к активу {AssetId} в чужом портфеле {PortfolioId}",
                        _userContext.UserId, id, asset.PortfolioId);
                    return null;
                }

                var cardInfo = await GetStockCardInfoAsync(asset.AssetType, asset.StockCardId, false);
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
                PortfolioAsset? asset = await _portfolioAssetRepository.GetByIdAsync(id);
                if (asset == null) return false;

                Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(asset.PortfolioId);
                if (portfolio == null) return false;

                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} пытается проверить существование актива {AssetId} в чужом портфеле",
                        _userContext.UserId, id);
                    return false;
                }

                return true;
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
                if (transaction == null)
                {
                    _logger.LogWarning("Транзакция с ID {TransactionId} не найдена", transactionId);
                    return null;
                }

                PortfolioAsset? asset = await _portfolioAssetRepository.GetByIdAsync(transaction.PortfolioAssetId);
                if (asset == null) return null;

                Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(asset.PortfolioId);
                if (portfolio == null) return null;

                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} пытается получить транзакцию {TransactionId} из чужого портфеля",
                        _userContext.UserId, transactionId);
                    return null;
                }

                return new PortfolioAssetTransactionDto(
                    transaction.Id, transaction.PortfolioAssetId, transaction.TransactionDate,
                    transaction.TransactionType, transaction.Quantity, transaction.PricePerUnit, transaction.Currency);
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
                PortfolioAsset? asset = await _portfolioAssetRepository.GetByIdAsync(assetId);
                if (asset == null)
                {
                    _logger.LogWarning("Актив с ID {AssetId} не найден", assetId);
                    return [];
                }

                Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(asset.PortfolioId);
                if (portfolio == null) return [];

                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} пытается получить транзакции актива {AssetId} из чужого портфеля",
                        _userContext.UserId, assetId);
                    return [];
                }

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
                PortfolioAsset? asset = await _portfolioAssetRepository.GetByIdAsync(assetId);
                if (asset == null) return [];

                Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(asset.PortfolioId);
                if (portfolio == null) return [];

                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} пытается получить транзакции актива {AssetId} за период из чужого портфеля",
                        _userContext.UserId, assetId);
                    return [];
                }

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
                PortfolioAsset? asset = await _portfolioAssetRepository.GetByIdAsync(assetId, a => a.Transactions) ?? throw new KeyNotFoundException($"Актив с ID {assetId} не найден");
                Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(asset.PortfolioId) ?? throw new KeyNotFoundException($"Портфель с ID {asset.PortfolioId} не найден");

                // Проверка прав доступа
                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} пытается добавить транзакцию к активу {AssetId} в чужом портфеле",
                        _userContext.UserId, assetId);
                    throw new KeyNotFoundException("Портфель не найден");
                }

                if (dto.Quantity <= 0)
                    throw new ArgumentOutOfRangeException(nameof(dto), "Количество должно быть больше нуля");

                if (dto.TransactionType == PortfolioAssetTransactionType.Sell && dto.Quantity > asset.TotalQuantity)
                    throw new InvalidOperationException("Недостаточно активов для продажи");

                var cardInfo = await GetStockCardInfoAsync(asset.AssetType, asset.StockCardId, false);

                PortfolioAssetTransaction transaction = new(
                    Guid.NewGuid(),
                    assetId,
                    dto.TransactionType,
                    dto.Quantity,
                    dto.PricePerUnit,
                    dto.TransactionDate ?? DateTime.UtcNow,
                    dto.Currency ?? cardInfo.Currency);

                PortfolioAssetTransaction createdTransaction = await _portfolioAssetRepository.AddAssetTransactionAsync(transaction);
                asset.Transactions.Add(transaction);

                if (asset.TotalQuantity == 0)
                {
                    await _portfolioAssetRepository.DeleteAsync(asset);
                    await InvalidatePortfolioCacheAsync(portfolio.Id);
                }

                await InvalidateAssetCacheAsync(assetId);

                return new PortfolioAssetTransactionDto(
                    createdTransaction.Id, createdTransaction.PortfolioAssetId, createdTransaction.TransactionDate,
                    createdTransaction.TransactionType, createdTransaction.Quantity, createdTransaction.PricePerUnit, createdTransaction.Currency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании транзакции");
                throw;
            }
        }

        /// <summary>
        /// Обновить транзакцию покупки/продажи актива ценной бумаги в портфеле
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="updatingTransactionDto"></param>
        /// <returns></returns>
        public async Task UpdateAssetTransactionAsync(
            Guid transactionId,
            UpdatingPortfolioAssetTransactionDto updatingTransactionDto)
        {
            _logger.LogInformation("Обновление транзакции {TransactionId}", transactionId);

            try
            {
                if (updatingTransactionDto.Quantity <= 0)
                    throw new ArgumentException("Количество должно быть больше нуля");
                if (updatingTransactionDto.PricePerUnit < 0)
                    throw new ArgumentException("Цена не может быть отрицательной");

                PortfolioAssetTransaction? transaction = await _portfolioAssetRepository.GetAssetTransactionByIdAsync(transactionId);
                if (transaction == null)
                {
                    _logger.LogWarning("Транзакция {TransactionId} не найдена", transactionId);
                    return;
                }

                PortfolioAsset? asset = await _portfolioAssetRepository.GetByIdAsync(transaction.PortfolioAssetId);
                if (asset == null)
                {
                    _logger.LogError("Актив для транзакции {TransactionId} не найден", transactionId);
                    throw new InvalidOperationException("Актив портфеля не найден");
                }

                Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(asset.PortfolioId);
                if (portfolio == null)
                {
                    _logger.LogWarning("Портфель {PortfolioId} для актива {AssetId} не найден", asset.PortfolioId, asset.Id);
                    throw new KeyNotFoundException("Портфель не найден");
                }

                // Проверка прав доступа
                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} пытается обновить транзакцию {TransactionId} в чужом портфеле",
                        _userContext.UserId, transactionId);
                    throw new SecurityException("Доступ запрещён");
                }

                transaction.TransactionType = updatingTransactionDto.TransactionType;
                transaction.Quantity = updatingTransactionDto.Quantity;
                transaction.PricePerUnit = updatingTransactionDto.PricePerUnit;
                transaction.TransactionDate = updatingTransactionDto.TransactionDate;
                transaction.Currency = updatingTransactionDto.Currency;

                await _portfolioAssetRepository.UpdateAssetTransactionAsync(transaction);
                await InvalidateAssetCacheAsync(transaction.PortfolioAssetId);

                _logger.LogInformation("Транзакция {TransactionId} успешно обновлена", transactionId);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении транзакции {TransactionId}", transactionId);
                throw;
            }
        }

        /// <summary>
        /// Получить информацию по доходности актива
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="calculationType"></param>
        /// <returns></returns>
        public async Task<PortfolioAssetProfitLossDto?> GetAssetProfitLossAsync(Guid assetId, CalculationType calculationType = CalculationType.Current)
        {
            try
            {
                _logger.LogInformation("Расчет доходности типа {CalculationType} для актива ID: {AssetId}", calculationType, assetId);
                PortfolioAsset? asset = await _portfolioAssetRepository.GetByIdAsync(assetId, a => a.Transactions);
                if (asset is null)
                {
                    _logger.LogWarning("Актив портфеля с ID {AssetId} не найден", assetId);
                    return null;
                }

                Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(asset.PortfolioId);
                if (portfolio == null) return null;

                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} пытается получить доходность актива {AssetId} в чужом портфеле",
                        _userContext.UserId, assetId);
                    return null;
                }

                var cardInfo = await GetStockCardInfoAsync(asset.AssetType, asset.StockCardId, true);

                return calculationType switch
                {
                    CalculationType.Realized => CalculateRealizedProfitLoss(asset, cardInfo),
                    _ => CalculateCurrentProfitLoss(asset, cardInfo)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при расчете доходности для актива ID: {AssetId}", assetId);
                throw;
            }
        }

        // --- Приватные методы для расчёта доходности (без изменений) ---

        private PortfolioAssetProfitLossDto CalculateCurrentProfitLoss(PortfolioAsset asset, StockCardInfoDto cardInfo)
        {
            var totalQuantity = asset.TotalQuantity;
            var averagePurchasePrice = asset.AveragePurchasePrice;
            var investmentAmount = totalQuantity * averagePurchasePrice;
            decimal? absoluteReturn = null;
            decimal? percentageReturn = null;
            decimal? currentValue = null;
            if (cardInfo.CurrentPrice.HasValue)
            {
                _logger.LogDebug("Расчет текущей доходности для актива {Ticker}: количество={Quantity}, средняя цена={AveragePrice}, текущая цена={CurrentPrice}",
                    cardInfo.Ticker, totalQuantity, averagePurchasePrice, cardInfo.CurrentPrice);
                currentValue = totalQuantity * cardInfo.CurrentPrice.Value;
                absoluteReturn = currentValue - investmentAmount;
                percentageReturn = investmentAmount == 0 ? 0 : decimal.Round((absoluteReturn ?? 0) / investmentAmount * 100);
            }
            else
                _logger.LogWarning("Текущая доходность по активу {Ticker} не расчитана, так как не задана его текущая цена", cardInfo.Ticker);

            return new PortfolioAssetProfitLossDto(
                asset.Id,
                asset.PortfolioId,
                cardInfo.Ticker,
                cardInfo.Name,
                absoluteReturn,
                percentageReturn,
                investmentAmount,
                currentValue,
                cardInfo.Currency,
                totalQuantity,
                averagePurchasePrice,
                cardInfo.CurrentPrice,
                CalculationType.Current);
        }

        private PortfolioAssetProfitLossDto CalculateRealizedProfitLoss(PortfolioAsset asset, StockCardInfoDto cardInfo)
        {
            var realizedProfitLoss = CalculateRealizedProfitLoss(asset.Transactions);
            var totalInvestment = asset.TotalInvestment;
            var percentageReturn = totalInvestment == 0 ? 0 : decimal.Round(realizedProfitLoss / totalInvestment * 100);

            _logger.LogDebug("Расчет реализованной доходности для актива {Ticker}: реализованная прибыль={RealizedProfit}, общие инвестиции={TotalInvestment}",
                cardInfo.Ticker, realizedProfitLoss, totalInvestment);

            return new PortfolioAssetProfitLossDto(
                asset.Id,
                asset.PortfolioId,
                cardInfo.Ticker,
                cardInfo.Name,
                realizedProfitLoss,
                percentageReturn,
                totalInvestment,
                0,
                cardInfo.Currency,
                0,
                0,
                0,
                CalculationType.Realized);
        }

        private decimal CalculateRealizedProfitLoss(IEnumerable<PortfolioAssetTransaction> transactions)
        {
            var sellTransactions = transactions
                .Where(t => t.TransactionType == PortfolioAssetTransactionType.Sell)
                .OrderBy(t => t.TransactionDate)
                .ToList();

            var buyTransactions = transactions
                .Where(t => t.TransactionType == PortfolioAssetTransactionType.Buy)
                .OrderBy(t => t.TransactionDate)
                .ToList();

            _logger.LogDebug("Расчет реализованной доходности: найдено {SellCount} продаж и {BuyCount} покупок",
                sellTransactions.Count, buyTransactions.Count);

            decimal realizedProfitLoss = 0;
            var availableBuys = new Queue<(int Quantity, decimal Price)>(buyTransactions.Select(b => (b.Quantity, b.PricePerUnit)));

            foreach (var sell in sellTransactions)
            {
                var remainingSellQuantity = sell.Quantity;

                while (remainingSellQuantity > 0 && availableBuys.Count > 0)
                {
                    var (Quantity, Price) = availableBuys.Peek();
                    var quantityToUse = Math.Min(Quantity, remainingSellQuantity);

                    realizedProfitLoss += quantityToUse * (sell.PricePerUnit - Price);

                    if (Quantity == quantityToUse)
                    {
                        availableBuys.Dequeue();
                    }
                    else
                    {
                        availableBuys.Dequeue();
                        availableBuys.Enqueue((Quantity - quantityToUse, Price));
                    }

                    remainingSellQuantity -= quantityToUse;
                }
            }

            _logger.LogDebug("Реализованная доходность рассчитана: {RealizedProfitLoss}", realizedProfitLoss);
            return realizedProfitLoss;
        }
    }
}
