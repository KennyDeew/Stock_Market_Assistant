using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using StockMarketAssistant.AnalyticsService.WebApi.Services;

namespace StockMarketAssistant.AnalyticsService.WebApi.Controllers;

/// <summary>
/// Управление тестовыми данными аналитики
/// </summary>
[ApiController]
[Route("api/test-data")]
[Produces("application/json")]
[OpenApiTag("Test Data Management", Description = "Операции для управления тестовыми данными: очистка базы данных, заполнение тестовыми транзакциями и рейтингами")]
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
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции очистки с информацией о количестве удаленных записей</returns>
    /// <remarks>
    /// Удаляет все транзакции и рейтинги из базы данных аналитики.
    ///
    /// Пример запроса:
    /// POST /api/test-data/clear
    ///
    /// Пример ответа:
    /// {
    ///   "success": true,
    ///   "message": "База данных успешно очищена. Удалено транзакций: 150, рейтингов: 25",
    ///   "transactionsDeleted": 150,
    ///   "ratingsDeleted": 25
    /// }
    /// </remarks>
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
    /// <param name="transactionId">ID конкретной транзакции для расчета рейтинга (опционально, можно передать через query или body). Если указан - используется эта транзакция. Если не указан - используются существующие транзакции из БД. Если транзакций нет - создаются фейковые.</param>
    /// <param name="request">Параметры генерации данных (опционально). Содержит TransactionId (строка) и DaysBack (количество дней назад для генерации фейковых транзакций, по умолчанию 90)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции заполнения с информацией о созданных транзакциях и рейтингах</returns>
    /// <remarks>
    /// Заполняет базу данных тестовыми данными. Поддерживает три режима работы:
    ///
    /// 1. По конкретной транзакции: если указан transactionId, используется эта транзакция для расчета рейтинга
    /// 2. По существующим транзакциям: если transactionId не указан, но в БД есть транзакции, используются все существующие транзакции
    /// 3. Генерация фейковых данных: если транзакций нет в БД, создаются фейковые компании, портфели и транзакции
    ///
    /// После добавления транзакций автоматически запускается расчет рейтингов активов.
    ///
    /// Пример запроса:
    /// POST /api/test-data/seed
    /// Content-Type: application/json
    /// {
    ///   "transactionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "daysBack": 90
    /// }
    ///
    /// Или через query параметр:
    /// POST /api/test-data/seed?transactionId=3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// Пример ответа:
    /// {
    ///   "success": true,
    ///   "message": "База данных успешно обработана. Обработано транзакций: 150",
    ///   "transactionsCreated": 150,
    ///   "buyTransactionsCount": 75,
    ///   "sellTransactionsCount": 75
    /// }
    /// </remarks>
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
    /// <param name="request">Параметры генерации данных. Содержит TransactionId (строка, опционально) и DaysBack (количество дней назад, по умолчанию 90)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции с информацией об удаленных и созданных записях</returns>
    /// <remarks>
    /// Выполняет две операции последовательно:
    /// 1. Очистка базы данных (удаление всех транзакций и рейтингов)
    /// 2. Заполнение базы данных тестовыми данными
    ///
    /// Параметры генерации данных аналогичны методу seed.
    ///
    /// Пример запроса:
    /// POST /api/test-data/reset
    /// Content-Type: application/json
    /// {
    ///   "daysBack": 90
    /// }
    ///
    /// Пример ответа:
    /// {
    ///   "success": true,
    ///   "message": "База данных сброшена и заполнена. База данных успешно очищена. Удалено транзакций: 150, рейтингов: 25. База данных успешно обработана. Обработано транзакций: 200",
    ///   "transactionsDeleted": 150,
    ///   "ratingsDeleted": 25,
    ///   "transactionsCreated": 200,
    ///   "buyTransactionsCount": 100,
    ///   "sellTransactionsCount": 100
    /// }
    /// </remarks>
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
    /// ID конкретной транзакции для расчета рейтинга (опционально, можно передать как строку в формате GUID)
    /// Если указан - используется эта транзакция для расчета рейтинга
    /// Если не указан - используются существующие транзакции из БД
    /// Если транзакций нет в БД - создаются фейковые транзакции
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Количество дней назад для генерации фейковых транзакций (если транзакций нет в БД)
    /// Допустимый диапазон: от 1 до 365 дней
    /// По умолчанию: 90 дней
    /// </summary>
    public int DaysBack { get; set; } = 90;
}

