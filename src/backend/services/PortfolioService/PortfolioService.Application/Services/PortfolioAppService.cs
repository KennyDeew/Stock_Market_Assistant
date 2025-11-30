using Microsoft.Extensions.Logging;
using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Caching;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Security;
using StockMarketAssistant.PortfolioService.Domain.Entities;
using StockMarketAssistant.PortfolioService.Domain.Enums;
using StockMarketAssistant.PortfolioService.Domain.Exceptions;
using StockMarketAssistant.SharedLibrary.Enums;

namespace StockMarketAssistant.PortfolioService.Application.Services
{
    /// <summary>
    /// Сервис работы с портфелями ценных бумаг
    /// </summary>
    public class PortfolioAppService(IPortfolioRepository portfolioRepository, IUserContext userContext, IPortfolioAssetAppService portfolioAssetAppService, ICacheService cache, ILogger<PortfolioAppService> logger) : IPortfolioAppService
    {
        private readonly IUserContext _userContext = userContext;
        private readonly IPortfolioRepository _portfolioRepository = portfolioRepository;
        private readonly IPortfolioAssetAppService _portfolioAssetAppService = portfolioAssetAppService;
        private readonly ICacheService _cache = cache;
        private readonly ILogger<PortfolioAppService> _logger = logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

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
        /// Создать портфель ценных бумаг
        /// </summary>
        /// <param name="creatingPortfolioDto">DTO создаваемого портфеля</param>
        /// <returns>Идентификатор созданного портфеля</returns>
        public async Task<Guid> CreateAsync(CreatingPortfolioDto creatingPortfolioDto)
        {
            if (_userContext.UserId == Guid.Empty)
                throw new SecurityException("Пользователь не аутентифицирован");

            // Проверка: если USER — то UserId должен совпадать
            if (!_userContext.IsAdmin && creatingPortfolioDto.UserId != _userContext.UserId)
            {
                _logger.LogWarning(
                    "Пользователь {CurrentUserId} (роль: {Role}) пытается создать портфель от имени {TargetUserId}",
                    _userContext.UserId, _userContext.Role ?? "UNKNOWN", creatingPortfolioDto.UserId);

                throw new SecurityException("Недопустимая операция: нельзя создавать портфели от имени других пользователей");
            }

            Portfolio portfolio = new(Guid.NewGuid(), creatingPortfolioDto.UserId, creatingPortfolioDto.Name, creatingPortfolioDto.Currency, creatingPortfolioDto.IsPrivate);
            try
            {
                Portfolio createdPortfolio = await _portfolioRepository.AddAsync(portfolio);
                return createdPortfolio.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании портфеля для пользователя {UserId} с именем {PortfolioName}",
                               creatingPortfolioDto.UserId, creatingPortfolioDto.Name);
                throw;
            }
        }

        /// <summary>
        /// Удалить портфель ценных бумаг
        /// </summary>
        /// <param name="id">Идентификатор портфеля</param>
        /// <returns>Успешность операции</returns>
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(id);
                if (portfolio is null)
                {
                    _logger.LogWarning("Портфель с ID {PortfolioId} не найден для удаления", id);
                    return false;
                }

                if (_userContext.UserId == Guid.Empty)
                    throw new SecurityException("Пользователь не аутентифицирован");

                // Проверка прав доступа: USER может удалять только свои портфели
                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} пытается удалить портфель от имени пользователя {TargetUserId}",
                        _userContext.UserId, portfolio.UserId);

                    throw new SecurityException("Доступ запрещён");
                }

                await _portfolioRepository.DeleteAsync(portfolio);
                await InvalidatePortfolioCacheAsync(id);
                await _cache.RemoveAsync($"user_portfolios_short_{portfolio.UserId}");

                _logger.LogInformation("Портфель {PortfolioId} успешно удалён пользователем {UserId}", id, _userContext.UserId);
                return true;
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении портфеля с ID {PortfolioId}", id);
                throw;
            }
        }

        /// <summary>
        /// Получить перечень всех портфелей ценных бумаг для всех пользователей
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PortfolioDto>> GetAllAsync()
        {
            // Только ADMIN может получать список всех портфелей
            if (!_userContext.IsAdmin)
            {
                _logger.LogWarning(
                    "Пользователь {CurrentUserId} пытается получить доступ к портфелям всех пользователей",
                    _userContext.UserId);

                throw new SecurityException("Доступ запрещён");
            }

            try
            {
                IEnumerable<Portfolio> portfolios = await _portfolioRepository.GetAllAsync();
                return portfolios.Select(p =>
                    new PortfolioDto
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        Name = p.Name,
                        Currency = p.Currency
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка всех портфелей");
                throw;
            }
        }

        /// <summary>
        /// Получить портфель ценных бумаг
        /// </summary>
        /// <param name="id">Идентификатор портфеля</param>
        /// <returns></returns>
        public async Task<PortfolioDto?> GetByIdAsync(Guid id)
        {
            var cacheKey = $"portfolio_{id}";
            try
            {
                // Попытка получить из кэша
                var cached = await _cache.GetAsync<PortfolioDto>(cacheKey);
                if (cached != null)
                    return cached;

                Portfolio? portfolio = await _portfolioRepository.GetByIdWithAssetsAsync(id);
                if (portfolio is null)
                {
                    _logger.LogWarning("Портфель с ID {PortfolioId} не найден", id);
                    return null;
                }

                if (_userContext.UserId == Guid.Empty)
                    throw new SecurityException("Пользователь не аутентифицирован");

                // Проверка прав доступа: USER может читать только свои портфели
                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} (роль: {Role}) пытается получить доступ к портфелю {PortfolioId}, принадлежащему {OwnerId}",
                        _userContext.UserId, _userContext.Role ?? "UNKNOWN", id, portfolio.UserId);

                    // Не возвращаем 403 — чтобы не раскрывать существование портфеля
                    // Возвращаем 404 — "не найден"
                    return null;
                }

                // Теперь загружаем активы
                var validAssets = new List<PortfolioAssetDto>();
                foreach (var asset in portfolio.Assets)
                {
                    try
                    {
                        var dto = await _portfolioAssetAppService.GetByIdAsync(asset.Id);
                        if (dto != null)
                            validAssets.Add(dto);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Не удалось загрузить актив {AssetId}", asset.Id);
                    }
                }
                var portfolioDto = new PortfolioDto
                {
                    Id = portfolio.Id,
                    UserId = portfolio.UserId,
                    Name = portfolio.Name,
                    Currency = portfolio.Currency,
                    Assets = [.. validAssets]
                };

                // Сохраняем в кэш — только после успешной проверки
                await _cache.SetAsync(cacheKey, portfolioDto, _cacheExpiration);
                return portfolioDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при получении портфеля по ID: {PortfolioId}", id);
                throw;
            }
        }

        /// <summary>
        /// Существует ли портфель ценных бумаг
        /// </summary>
        /// <param name="id">Идентификатор портфеля</param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                var portfolio = await _portfolioRepository.GetByIdAsync(id);
                if (portfolio == null) return false;

                if (_userContext.UserId == Guid.Empty)
                    throw new SecurityException("Пользователь не аутентифицирован");

                // Проверка доступа: либо владелец, либо админ
                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} пытается проверить существование чужого портфеля {PortfolioId}",
                        _userContext.UserId, id);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке существования портфеля с ID: {PortfolioId}", id);
                return false;
            }
        }

        /// <summary>
        /// Редактировать портфель
        /// </summary>
        /// <param name="id">Идентификатор портфеля</param>
        /// <param name="updatingPortfolioDto">DTO для редактирования портфеля</param>
        public async Task UpdateAsync(Guid id, UpdatingPortfolioDto updatingPortfolioDto)
        {
            try
            {
                // Получаем портфель из БД
                Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(id);
                if (portfolio == null)
                {
                    _logger.LogWarning("Попытка обновить несуществующий портфель с ID: {PortfolioId}", id);
                    throw new KeyNotFoundException($"Портфель с ID {id} не найден");
                }

                if (_userContext.UserId == Guid.Empty)
                    throw new SecurityException("Пользователь не аутентифицирован");

                // Проверка прав доступа: USER может редактировать только свои портфели
                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} (роль: {Role}) пытается изменить портфель {PortfolioId}, принадлежащий {OwnerId}",
                        _userContext.UserId, _userContext.Role ?? "UNKNOWN", id, portfolio.UserId);

                    // Не раскрываем существование портфеля — выбрасываем KeyNotFoundException
                    throw new KeyNotFoundException($"Портфель с ID {id} не найден");
                }

                // Обновляем данные
                portfolio.Name = updatingPortfolioDto.Name;
                portfolio.Currency = updatingPortfolioDto.Currency;
                portfolio.IsPrivate = updatingPortfolioDto.IsPrivate;

                // Сохраняем изменения
                await _portfolioRepository.UpdateAsync(portfolio);

                // Сбрасываем кэш
                await InvalidatePortfolioCacheAsync(id);

                _logger.LogInformation("Портфель {PortfolioId} успешно обновлён пользователем {UserId}", id, _userContext.UserId);
            }
            catch (KeyNotFoundException)
            {
                // Перебрасываем — будет обработано в контроллере как NotFound (404)
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при обновлении портфеля с ID: {PortfolioId}", id);
                throw;
            }
        }

        /// <summary>
        /// Получить доходность портфеля по его ID с детализацией по активам
        /// </summary>
        /// <param name="portfolioId"></param>
        /// <param name="calculationType"></param>
        /// <returns></returns>
        public async Task<PortfolioProfitLossDto?> GetPortfolioProfitLossAsync(Guid id, CalculationType calculationType = CalculationType.Current)
        {
            try
            {
                _logger.LogInformation("Расчет доходности портфеля ID: {PortfolioId}, тип расчета: {CalculationType}",
                    id, calculationType);

                Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(id);
                if (portfolio is null)
                {
                    _logger.LogWarning("Портфель с ID {PortfolioId} не найден", id);
                    return null;
                }

                if (_userContext.UserId == Guid.Empty)
                    throw new SecurityException("Пользователь не аутентифицирован");

                // Проверка прав доступа: USER может читать только свои портфели
                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} пытается получить доходность чужого портфеля {PortfolioId}",
                        _userContext.UserId, id);
                    return null;
                }

                // Получаем доходности всех активов портфеля
                var portfolioAssetsProfitLoss = await GetPortfolioAssetsProfitLossAsync(id, calculationType);
                var assetList = portfolioAssetsProfitLoss.ToList();

                if (assetList.Count == 0)
                {
                    _logger.LogWarning("В портфеле {PortfolioId} не найдено активов для расчета доходности", id);
                    return new PortfolioProfitLossDto(
                        portfolio.Id,
                        portfolio.Name,
                        0, 0, 0, 0,
                        portfolio.Currency ?? "RUB",
                        DateTime.UtcNow,
                        []);
                }

                // Агрегируем данные портфеля
                var totalInvestment = assetList.Sum(a => a.InvestmentAmount);
                var totalCurrentValue = assetList.Sum(a => a.CurrentValue);
                var totalAbsoluteReturn = totalCurrentValue - totalInvestment;
                var totalPercentageReturn = totalInvestment != 0 ? decimal.Round((totalAbsoluteReturn ?? 0) / totalInvestment * 100) : 0;

                _logger.LogDebug("Агрегация метрик портфеля: инвестиции={Investment}, текущая стоимость={CurrentValue}",
                    totalInvestment, totalCurrentValue);

                _logger.LogInformation("Доходность портфеля {PortfolioId} рассчитана: абсолютная={AbsoluteReturn}, процентная={PercentageReturn}%",
                    id, totalAbsoluteReturn, totalPercentageReturn);

                return new PortfolioProfitLossDto(
                    portfolio.Id,
                    portfolio.Name,
                    totalAbsoluteReturn ?? 0,
                    totalPercentageReturn,
                    totalInvestment,
                    totalCurrentValue ?? 0,
                    portfolio.Currency ?? "RUB",
                    DateTime.UtcNow,
                    assetList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при расчете доходности портфеля ID: {PortfolioId}", id);
                throw;
            }
        }

        /// <summary>
        /// Получить расчет доходности по всем активам портфеля
        /// </summary>
        /// <param name="portfolioId">Уникальный идентификатор портфеля</param>
        /// <param name="calculationType">Тип расчета доходности (по умолчанию - текущая доходность)</param>
        /// <returns>Коллекция DTO с расчетами доходности по каждому активу портфеля</returns>
        public async Task<IEnumerable<PortfolioAssetProfitLossItemDto>> GetPortfolioAssetsProfitLossAsync(Guid id, CalculationType calculationType = CalculationType.Current)
        {
            try
            {
                _logger.LogInformation("Получение доходности активов портфеля ID: {id}", id);

                Portfolio? portfolio = await _portfolioRepository.GetByIdWithAssetsAndTransactionsAsync(id);
                if (portfolio == null)
                {
                    _logger.LogWarning("Портфель с ID {id} не найден", id);
                    return [];
                }

                if (_userContext.UserId == Guid.Empty)
                    throw new SecurityException("Пользователь не аутентифицирован");

                // Проверка прав доступа
                if (!_userContext.IsAdmin && portfolio.UserId != _userContext.UserId)
                {
                    _logger.LogWarning("Пользователь {CurrentUserId} пытается получить доходность активов чужого портфеля {PortfolioId}", _userContext.UserId, id);
                    return [];
                }

                if (portfolio.Assets.Count == 0)
                {
                    _logger.LogWarning("В портфеле {id} не найдено активов", id);
                    return [];
                }

                decimal portfolioTotalValue = 0;
                var assetsProfitLoss = new List<PortfolioAssetProfitLossItemDto>();

                foreach (var asset in portfolio.Assets)
                {
                    try
                    {
                        var cardInfo = await _portfolioAssetAppService.GetStockCardInfoAsync(asset.AssetType, asset.StockCardId);
                        if (cardInfo.CurrentPrice.HasValue)
                            portfolioTotalValue += asset.TotalQuantity * cardInfo.CurrentPrice.Value;
                        else
                            _logger.LogWarning("Не удалось получить цену для актива {Ticker}, пропускаем в расчете общей стоимости", cardInfo.Ticker);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Не удалось получить цену для актива c ID {assetId}, пропускаем в расчете общей стоимости", asset.Id);
                    }
                }
                _logger.LogDebug("Общая стоимость портфеля рассчитана: {TotalValue}", portfolioTotalValue);

                foreach (var asset in portfolio.Assets)
                {
                    var assetProfitLoss = await CalculatePortfolioAssetProfitLossAsync(asset, calculationType, portfolioTotalValue);
                    if (assetProfitLoss != null)
                    {
                        assetsProfitLoss.Add(assetProfitLoss);
                    }
                }

                _logger.LogInformation("Успешно рассчитана доходность для {AssetCount} активов портфеля {id}",
                    assetsProfitLoss.Count, id);

                return assetsProfitLoss;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении доходности активов портфеля ID: {id}", id);
                throw;
            }
        }

        /// <summary>
        /// Расчет доходности для отдельного актива в контексте портфеля
        /// </summary>
        /// <param name="asset">Актив портфеля</param>
        /// <param name="calculationType">Тип расчета доходности</param>
        /// <param name="portfolioTotalValue">Общая стоимость портфеля для расчета веса актива</param>
        /// <returns>DTO с расчетом доходности актива в контексте портфеля</returns>
        private async Task<PortfolioAssetProfitLossItemDto?> CalculatePortfolioAssetProfitLossAsync(
            PortfolioAsset asset,
            CalculationType calculationType,
            decimal portfolioTotalValue)
        {
            try
            {
                StockCardInfoDto cardInfo = await _portfolioAssetAppService.GetStockCardInfoAsync(asset.AssetType, asset.StockCardId);
                int totalQuantity = asset.TotalQuantity;
                decimal averagePurchasePrice = asset.AveragePurchasePrice;
                decimal? currentValue = cardInfo.CurrentPrice.HasValue ? totalQuantity * cardInfo.CurrentPrice.Value : null;
                decimal investmentAmount = totalQuantity * averagePurchasePrice;
                decimal absoluteReturn = 0;
                decimal percentageReturn = 0;

                if (calculationType == CalculationType.Realized)
                {
                    decimal realizedProfitLoss = CalculateRealizedProfitLoss(asset.Transactions);
                    absoluteReturn = realizedProfitLoss;
                    percentageReturn = investmentAmount != 0 ? decimal.Round(absoluteReturn / investmentAmount * 100) : 0;
                    currentValue = 0;
                    totalQuantity = 0;
                }
                else if (currentValue.HasValue)
                {
                    absoluteReturn = currentValue.Value - investmentAmount;
                    percentageReturn = investmentAmount == 0 ? 0 : decimal.Round(absoluteReturn / investmentAmount * 100);
                }

                decimal? weightInPortfolio = portfolioTotalValue == 0 ? 0 : decimal.Round((currentValue ?? 0) / portfolioTotalValue * 100);

                return new PortfolioAssetProfitLossItemDto(
                    asset.Id,
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
                    weightInPortfolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при расчете доходности актива {AssetId} портфеля", asset.Id);
                return null;
            }
        }

        /// <summary>
        /// Расчет реализованной доходности по транзакциям (метод FIFO)
        /// </summary>
        /// <param name="transactions">Коллекция транзакций актива</param>
        /// <returns>Реализованная доходность</returns>
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

        /// <summary>
        /// Получить перечень всех портфелей ценных бумаг для пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns></returns>
        public async Task<IEnumerable<PortfolioShortDto>> GetByUserIdAsync(Guid userId)
        {
            var cacheKey = $"user_portfolios_short_{userId}";
            try
            {
                // Пробуем получить из кэша
                var cached = await _cache.GetAsync<IEnumerable<PortfolioShortDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogDebug("Кэш найден для портфелей пользователя {UserId}", userId);
                    return cached;
                }

                if (_userContext.UserId == Guid.Empty)
                    throw new SecurityException("Пользователь не аутентифицирован");

                // Проверка: USER может запрашивать только свои данные
                if (!_userContext.IsAdmin && _userContext.UserId != userId)
                {
                    _logger.LogWarning(
                        "Пользователь {CurrentUserId} пытается получить портфели пользователя {TargetUserId}",
                        _userContext.UserId, userId);

                    // Не возвращаем 403 — просто "не найдено"
                    return [];
                }

                var portfolios = await _portfolioRepository.GetByUserIdAsync(userId);

                if (!portfolios.Any())
                {
                    _logger.LogDebug("У пользователя {UserId} не найдено портфелей", userId);
                    return [];
                }

                var result = portfolios.Select(p => new PortfolioShortDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Name = p.Name,
                    Currency = p.Currency
                }).ToList();

                await _cache.SetAsync(cacheKey, result, _cacheExpiration);

                _logger.LogDebug("Получено {Count} портфелей для пользователя {UserId} (краткая версия)",
                    result.Count, userId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении краткой информации о портфелях пользователя {UserId}", userId);
                throw;
            }
        }
    }
}
