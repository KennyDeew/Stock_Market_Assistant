using Microsoft.Extensions.Logging;
using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Gateways;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Security;
using StockMarketAssistant.PortfolioService.Domain.Entities;
using StockMarketAssistant.PortfolioService.Domain.Enums;
using StockMarketAssistant.PortfolioService.Domain.Exceptions;
using StockMarketAssistant.SharedLibrary.Models;
using System.Text.Json;

namespace StockMarketAssistant.PortfolioService.Application.Services
{
    /// <summary>
    /// Сервис для работы с оповещениями
    /// </summary>
    public class AlertAppService(
        IAlertRepository alertRepository,
        IOutboxRepository outboxRepository,
        IPortfolioAssetAppService portfolioAssetAppService,
        IStockCardServiceGateway stockCardServiceGateway,
        IUserContext userContext,
        ILogger<AlertAppService> logger) : IAlertAppService
    {
        private const string _topicName = "notifications_send"; // наименование Kafka-топика, в который публикуются сообщения
        /// <inheritdoc/>
        public async Task<Guid> CreateAsync(CreatingAlertDto dto)
        {
            if (await alertRepository.ExistsByStockCardIdAsync(dto.StockCardId, userContext.UserId))
                throw new InvalidOperationException("Оповещение для этого актива уже существует");

            var alert = new Alert(
                Guid.NewGuid(),
                dto.StockCardId,
                dto.AssetType,
                dto.AssetTicker ?? "Unknown",
                dto.AssetName ?? "Unknown",
                dto.TargetPrice,
                dto.AssetCurrency ?? "RUB",
                dto.Condition,
                userContext.UserId,
                userContext.Email);

            var created = await alertRepository.AddAsync(alert);
            logger.LogInformation("Создано новое оповещение {AlertId} для пользователя {UserId}", alert.Id, alert.UserId);
            return created.Id;
        }

        /// <inheritdoc/>
        public async Task<AlertDto?> GetByIdAsync(Guid id)
        {
            Alert? alert = await alertRepository.GetByIdAsync(id);
            if (alert is null)
            {
                logger.LogWarning("Оповещение с ID {AlertId} не найдено", id);
                return null;
            }
            var alertDto = new AlertDto(
                alert.Id,
                alert.StockCardId,
                alert.AssetTicker,
                alert.AssetName,
                alert.AssetType,
                alert.TargetPrice,
                alert.AssetCurrency,
                alert.Condition,
                alert.IsActive,
                alert.CreatedAt.LocalDateTime,
                alert.UpdatedAt.LocalDateTime,
                alert.TriggeredAt?.LocalDateTime,
                alert.UserId,
                alert.LastChecked?.LocalDateTime);
            return alertDto;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AlertDto>> GetAllPendingAlertsAsync(Guid userId)
        {
            if (userId != userContext.UserId && !userContext.IsAdmin)
                throw new SecurityException("Доступ запрещён");

            var alerts = await alertRepository.GetActiveByUserIdAsync(userId);
            return alerts.Select(a => new AlertDto(
                a.Id,
                a.StockCardId,
                a.AssetTicker,
                a.AssetName,
                a.AssetType,
                a.TargetPrice,
                a.AssetCurrency,
                a.Condition,
                a.IsActive,
                a.CreatedAt.LocalDateTime,
                a.UpdatedAt.LocalDateTime,
                a.TriggeredAt?.LocalDateTime,
                a.UserId,
                a.LastChecked?.LocalDateTime));
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var alert = await alertRepository.GetByIdAsync(id);
            if (alert is null)
                return false;

            if (alert.UserId != userContext.UserId && !userContext.IsAdmin)
                throw new SecurityException("Доступ запрещён");

            await alertRepository.DeleteAsync(alert);
            logger.LogInformation("Удалено оповещение {AlertId} для пользователя {UserId}", alert.Id, alert.UserId);
            return true;
        }

        /// <inheritdoc/>
        public async Task ProcessPendingAlertsAsync()
        {
            try
            {
                logger.LogInformation("Начало обработки ожидающих оповещений");

                var pendingAlerts = await alertRepository.GetPendingAlertsAsync();
                int pendingAlertsCount = pendingAlerts.Count();

                logger.LogInformation("Найдено {Count} активных оповещений для обработки", pendingAlertsCount);

                if (pendingAlertsCount > 0)
                {
                    // Принудительное обновление цен
                    if (pendingAlerts.Any(a => a.AssetType == PortfolioAssetType.Share))
                        await stockCardServiceGateway.UpdateAllPricesForShareCardsAsync();
                    if (pendingAlerts.Any(a => a.AssetType == PortfolioAssetType.Bond))
                        await stockCardServiceGateway.UpdateAllPricesForBondCardsAsync();
                    foreach (var alert in pendingAlerts)
                    {
                        await ProcessSingleAlertAsync(alert);
                    }
                    logger.LogInformation("Завершена обработка ожидающих оповещений");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при обработке ожидающих оповещений");
                throw;
            }
        }

        /// <summary>
        /// Обработать одно оповещение
        /// </summary>
        /// <param name="alert">Оповещение</param>
        /// <param name="cancellationToken">Токен отмены</param>
        private async Task ProcessSingleAlertAsync(Alert alert)
        {
            try
            {
                // Получаем текущую цену из сервиса котировок
                decimal? currentPrice = null;
                try
                {
                    var cardInfo = await portfolioAssetAppService.GetStockCardInfoAsync(alert.AssetType, alert.StockCardId);
                    if (cardInfo.CurrentPrice.HasValue)
                        currentPrice = cardInfo.CurrentPrice.Value;
                    else
                        logger.LogWarning("Не удалось получить цену для актива {Ticker}", cardInfo.Ticker);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Не удалось получить цену для актива {Ticker}", alert.AssetTicker);
                }

                alert.UpdateLastChecked();
                // Сохраняем изменения
                await alertRepository.UpdateAsync(alert);

                if (currentPrice.HasValue)
                {
                    bool shouldTrigger = alert.Condition == AlertCondition.Above
                    ? currentPrice >= alert.TargetPrice
                    : currentPrice <= alert.TargetPrice;

                    if (shouldTrigger)
                    {
                        if (!alert.IsActive)
                        {
                            logger.LogWarning("Оповещение {AlertId} уже неактивно", alert.Id);
                            return;
                        }

                        if (string.IsNullOrEmpty(alert.UserEmail))
                        {
                            logger.LogWarning("Для оповещения {AlertId} не указан Email", alert.Id);
                            return;
                        }
                        alert.MarkAsTriggered();
                        // Создаём сообщение для Outbox
                        var emailNotification = new NotificationSendRequest
                        {
                            Recipient = alert.UserEmail,
                            IsHtml = true,
                            Subject = "Оповещение о достижении целевой цены",
                            Type = "Alert",
                            Parameters = new()
                            {
                                ["ticker"] = alert.AssetTicker,
                                ["asset"] = alert.AssetName,
                                ["condition"] = $"{alert.Condition.GetEnumDescription()} {alert.TargetPrice} {alert.AssetCurrency}",
                                ["current_price"] = $"{currentPrice} {alert.AssetCurrency}",
                                ["triggered_at"] = alert.TriggeredAt!.Value.LocalDateTime.ToString()
                            }
                        };

                        var outboxMessage = new OutboxMessage(
                            Guid.NewGuid(),
                            _topicName,
                            JsonSerializer.Serialize(emailNotification));

                        await outboxRepository.AddAsync(outboxMessage);

                        // Сохраняем изменения
                        await alertRepository.UpdateAsync(alert);
                        logger.LogInformation("Оповещение {AlertId} активировано и поставлено в очередь", alert.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка обработки оповещения {AlertId}", alert.Id);
            }
        }
    }
}
