using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockMarketAssistant.PortfolioService.Application.Interfaces;

namespace StockMarketAssistant.PortfolioService.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Фоновый сервис для обработки оповещений
    /// </summary>
    /// <remarks>
    /// Конструктор сервиса обработки оповещений
    /// </remarks>
    /// <param name="serviceProvider">Провайдер сервисов</param>
    /// <param name="logger">Логгер</param>
    public class AlertProcessingService(
        IServiceProvider serviceProvider,
        ILogger<AlertProcessingService> logger) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<AlertProcessingService> _logger = logger;
        private readonly TimeSpan _processingInterval = TimeSpan.FromMinutes(2.5);

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Сервис обработки оповещений запущен");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var alertService = scope.ServiceProvider.GetRequiredService<IAlertAppService>();
                        await alertService.ProcessPendingAlertsAsync();
                    }
                    _logger.LogDebug("Цикл обработки оповещений завершен");
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Ошибка во время обработки оповещений");
                }

                await Task.Delay(_processingInterval, stoppingToken);
            }
            _logger.LogInformation("Сервис обработки оповещений остановлен");
        }
    }
}
