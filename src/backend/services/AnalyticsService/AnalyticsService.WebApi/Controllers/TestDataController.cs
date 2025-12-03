using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using StockMarketAssistant.AnalyticsService.WebApi.Services;

namespace StockMarketAssistant.AnalyticsService.WebApi.Controllers;

/// <summary>
/// Контроллер для управления тестовыми данными аналитики
/// </summary>
[ApiController]
[Route("api/test-data")]
[Produces("application/json")]
[OpenApiTag("Test Data Management")]
public class TestDataController : ControllerBase
{
    private readonly TestDataService _testDataService;
    private readonly ILogger<TestDataController> _logger;

    public TestDataController(
        TestDataService testDataService,
        ILogger<TestDataController> logger)
    {
        _testDataService = testDataService ?? throw new ArgumentNullException(nameof(testDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Очистить базу данных аналитики (удалить все транзакции и рейтинги)
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат операции очистки</returns>
    [HttpPost("clear")]
    [ProducesResponseType(typeof(TestDataOperationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TestDataOperationResult), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TestDataOperationResult>> ClearDatabaseAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Запрос на очистку базы данных аналитики");

            var result = await _testDataService.ClearDatabaseAsync(cancellationToken);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при очистке базы данных");
            return StatusCode(StatusCodes.Status500InternalServerError, new TestDataOperationResult
            {
                Success = false,
                Message = $"Внутренняя ошибка: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Заполнить базу данных тестовыми данными
    /// </summary>
    /// <param name="transactionId">ID конкретной транзакции (опционально, можно передать через query или body)</param>
    /// <param name="request">Параметры генерации данных (опционально)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат операции заполнения</returns>
    [HttpPost("seed")]
    [ProducesResponseType(typeof(TestDataOperationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TestDataOperationResult), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TestDataOperationResult>> SeedDatabaseAsync(
        [FromQuery] Guid? transactionId = null,
        [FromBody] SeedDatabaseRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Приоритет: query параметр > body параметр
            Guid? finalTransactionId = transactionId;

            // Если в query нет, пытаемся парсить из body
            if (!finalTransactionId.HasValue && !string.IsNullOrWhiteSpace(request?.TransactionId))
            {
                if (Guid.TryParse(request.TransactionId, out var parsedGuid))
                {
                    finalTransactionId = parsedGuid;
                }
                else
                {
                    // Если не удалось распарсить - игнорируем и используем режим работы с существующими транзакциями
                    _logger.LogWarning("Некорректный формат TransactionId: '{TransactionId}'. Будет использован режим работы с существующими транзакциями.", request.TransactionId);
                    finalTransactionId = null;
                }
            }

            var daysBack = request?.DaysBack ?? 90;

            // Валидация параметров
            if (daysBack < 1 || daysBack > 365)
            {
                return BadRequest(new TestDataOperationResult
                {
                    Success = false,
                    Message = "Количество дней должно быть от 1 до 365"
                });
            }

            _logger.LogInformation(
                "Запрос на заполнение базы данных тестовыми данными. TransactionId: {TransactionId}, Дней назад: {DaysBack}",
                finalTransactionId?.ToString() ?? "не указан", daysBack);

            var result = await _testDataService.SeedDatabaseAsync(
                finalTransactionId,
                daysBack,
                cancellationToken);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при заполнении базы данных тестовыми данными");
            return StatusCode(StatusCodes.Status500InternalServerError, new TestDataOperationResult
            {
                Success = false,
                Message = $"Внутренняя ошибка: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Очистить и заполнить базу данных тестовыми данными (комбинированная операция)
    /// </summary>
    /// <param name="request">Параметры генерации данных</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат операции</returns>
    [HttpPost("reset")]
    [ProducesResponseType(typeof(TestDataOperationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TestDataOperationResult), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TestDataOperationResult>> ResetDatabaseAsync(
        [FromBody] SeedDatabaseRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Запрос на сброс базы данных (очистка + заполнение)");

            // Сначала очищаем
            var clearResult = await _testDataService.ClearDatabaseAsync(cancellationToken);
            if (!clearResult.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, clearResult);
            }

            // Затем заполняем
            Guid? transactionId = null;
            if (!string.IsNullOrWhiteSpace(request?.TransactionId))
            {
                if (Guid.TryParse(request.TransactionId, out var parsedGuid))
                {
                    transactionId = parsedGuid;
                }
                else
                {
                    // Если не удалось распарсить - игнорируем и используем режим работы с существующими транзакциями
                    _logger.LogWarning("Некорректный формат TransactionId: '{TransactionId}'. Будет использован режим работы с существующими транзакциями.", request.TransactionId);
                    transactionId = null;
                }
            }

            var daysBack = request?.DaysBack ?? 90;

            // Валидация параметров
            if (daysBack < 1 || daysBack > 365)
            {
                return BadRequest(new TestDataOperationResult
                {
                    Success = false,
                    Message = "Количество дней должно быть от 1 до 365"
                });
            }

            var seedResult = await _testDataService.SeedDatabaseAsync(
                transactionId,
                daysBack,
                cancellationToken);

            if (seedResult.Success)
            {
                // Объединяем результаты
                seedResult.Message = $"База данных сброшена и заполнена. {clearResult.Message}. {seedResult.Message}";
                seedResult.TransactionsDeleted = clearResult.TransactionsDeleted;
                seedResult.RatingsDeleted = clearResult.RatingsDeleted;
                return Ok(seedResult);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, seedResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сбросе базы данных");
            return StatusCode(StatusCodes.Status500InternalServerError, new TestDataOperationResult
            {
                Success = false,
                Message = $"Внутренняя ошибка: {ex.Message}"
            });
        }
    }
}

/// <summary>
/// Запрос на заполнение базы данных тестовыми данными
/// </summary>
public class SeedDatabaseRequest
{
    /// <summary>
    /// ID конкретной транзакции для расчета рейтинга (опционально, можно передать как строку)
    /// Если указан - используется эта транзакция
    /// Если не указан - используются существующие транзакции из БД
    /// Если транзакций нет - создаются фейковые
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Количество дней назад для генерации фейковых транзакций (если транзакций нет в БД, по умолчанию: 90)
    /// </summary>
    public int DaysBack { get; set; } = 90;
}

