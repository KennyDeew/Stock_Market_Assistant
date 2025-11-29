using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.Domain.Exceptions;
using StockMarketAssistant.PortfolioService.WebApi.Mappings;
using StockMarketAssistant.PortfolioService.WebApi.Models;
using System.Security.Claims;

namespace StockMarketAssistant.PortfolioService.WebApi.Controllers
{
    /// <summary>
    /// Контроллер для управления уведомлениями о ценах активов.
    /// Позволяет пользователю создавать, просматривать и удалять уведомления.
    /// </summary>
    [ApiController]
    [Route("api/v1/alerts")]
    [Produces("application/json")]
    [OpenApiTag("Alerts")]
    [Authorize(Roles = "USER")]
    public class AlertsController(IAlertAppService alertAppService, ILogger<AlertsController> logger) : ControllerBase
    {
        /// <summary>
        /// Получить все необработанные уведомления текущего пользователя
        /// </summary>
        /// <returns>Список уведомлений</returns>
        /// <response code="200">Успешно возвращён список уведомлений</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AlertResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AlertResponse>>> GetAll()
        {
            try
            {
                // Извлечение идентификатора пользователя из JWT-токена
                var userId = GetUserIdFromClaims();
                if (userId == Guid.Empty)
                    return Unauthorized(new { error = "Unauthorized", message = "Не удалось определить пользователя" });

                // Получение DTO из Application-слоя
                var alerts = await alertAppService.GetAllPendingAlertsAsync(userId);

                // Маппинг в Response-модель
                var responses = alerts.Select(a => a.ToResponse()).ToList();

                return Ok(responses);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при получении уведомлений для пользователя {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { error = "InternalError", message = "Внутренняя ошибка сервиса" });
            }
        }

        /// <summary>
        /// Создать новое уведомление для актива в портфеле
        /// </summary>
        /// <param name="request">Данные для создания уведомления</param>
        /// <returns>Созданное уведомление</returns>
        /// <response code="201">Уведомление успешно создано</response>
        /// <response code="400">Некорректные данные</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpPost]
        [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AlertResponse>> Create([FromBody] CreateAlertRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ValidationProblemDetails(ModelState));

            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == Guid.Empty)
                    return Unauthorized();

                // Передача данных в Application-сервис
                var dto = new CreatingAlertDto(request.StockCardId, request.AssetType, request.AssetTicker ?? string.Empty, request.AssetName ?? string.Empty, request.TargetPrice, request.AssetCurrency ?? "RUB", request.Condition);
                var alertId = await alertAppService.CreateAsync(dto);

                // Получаем полное уведомление (с заполненными данными)
                var createdAlert = await alertAppService.GetByIdAsync(alertId);

                if (createdAlert is null)
                    return StatusCode(500, new { error = "CreationError", message = "Уведомление создано, но не найдено" });

                var response = createdAlert.ToResponse();

                // Возвращаем 201 Created
                return CreatedAtAction(nameof(GetAll), new { id = alertId }, response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "NotFound", message = "Актив не найден в портфеле" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = "InvalidOperation", message = ex.Message });
            }
            catch (SecurityException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при создании уведомления для пользователя {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { error = "InternalError", message = "Внутренняя ошибка сервиса" });
            }
        }

        /// <summary>
        /// Удалить уведомление по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор уведомления</param>
        /// <returns>204 No Content или 404</returns>
        /// <response code="204">Уведомление успешно удалено</response>
        /// <response code="404">Уведомление не найдено</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == Guid.Empty)
                    return Unauthorized();

                var result = await alertAppService.DeleteAsync(id);

                if (!result)
                    return NotFound(new { error = "NotFound", message = "Уведомление не найдено" });

                return NoContent();
            }
            catch (SecurityException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при удалении уведомления {AlertId} для пользователя {UserId}", id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { error = "InternalError", message = "Внутренняя ошибка сервиса" });
            }
        }

        #region Helpers

        /// <summary>
        /// Извлечение UserId из JWT-токена
        /// </summary>
        private Guid GetUserIdFromClaims()
        {
            // Сначала пробуем стандартный способ
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Если не нашли — ищем по имени "Id"
            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = User.FindFirst("Id")?.Value;
            }

            return !string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId)
                ? userId
                : Guid.Empty;
        }

        #endregion
    }
}
