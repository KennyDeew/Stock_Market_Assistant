using Microsoft.AspNetCore.Mvc;
using StockCardService.WebApi.Models._01sub_FinancialReport;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.WebApi.Mappers;

namespace StockCardService.WebApi.Controllers
{
    /// <summary>
    /// Финансовые отчеты
    /// </summary>
    [Route("api/v1/[controller]")]
    public class FinancialReportController : ControllerBase
    {
        private readonly IFinancialReportService _financialReportService;

        /// <summary>
        /// Конструктор контроллера финансового отчета
        /// </summary>
        /// <param name="financialReportService"></param>
        public FinancialReportController(IFinancialReportService financialReportService)
        {
            _financialReportService = financialReportService;
        }

        /// <summary>
        /// Получить все финансовые отчеты всех акций
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<FinancialReportModel>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<List<FinancialReportModel>>> GetAllFinancialReportsAsync()
        {
            var financialReports = (await _financialReportService.GetAllAsync()).Select(FinancialReportMapper.ToModel).ToList();
            return Ok(financialReports);
        }

        /// <summary>
        /// Получить все Финансовые отчеты определенной акции
        /// </summary>
        /// <returns></returns>
        [HttpGet("ByShareCard/{shareCardId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<FinancialReportModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<List<FinancialReportModel>>> GetAllFinancialReportsByParentIdAsync(Guid shareCardId)
        {
            var financialReports = (await _financialReportService.GetAllByShareCardIdAsync(shareCardId)).Select(FinancialReportMapper.ToModel).ToList();
            if (!financialReports.Any())
                return NotFound();
            else
                return Ok(financialReports);
        }

        /// <summary>
        /// Получить финансовый отчет по Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetFinancialReportById")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FinancialReportModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<FinancialReportModel>> GetFinancialReportByIdAsync(Guid id)
        {
            var financialReportDto = await _financialReportService.GetByIdAsync(id);

            if (financialReportDto == null)
                return NotFound();

            var financialReportModel = FinancialReportMapper.ToModel(financialReportDto);
            return Ok(financialReportModel);
        }

        /// <summary>
        /// Добавить новый финансовый отчет
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(FinancialReportModel), 201)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> CreateFinancialReport(CreatingFinancialReportModel request)
        {
            var dto = FinancialReportMapper.ToDto(request);
            if (dto == null)
                return BadRequest("Запрос пустой");
            var createdFinancialReportId = await _financialReportService.CreateAsync(dto);
            if (createdFinancialReportId == Guid.Empty) return Problem("Не удалось создать финансовый отчет");

            var financialReportModel = new FinancialReportModel()
            {
                Id = createdFinancialReportId,
                ParentId = request.ParentId,
                Name = request.Name,
                Description = request.Description,
                Period = request.Period,
                Revenue = request.Revenue,
                EBITDA = request.EBITDA,
                NetProfit = request.NetProfit,
                CAPEX = request.CAPEX,
                FCF = request.FCF,
                Debt = request.Debt,
                TotalAssets = request.TotalAssets,
                NonCurrentAssets = request.NonCurrentAssets,
                CurrentAssets = request.CurrentAssets,
                Inventories = request.Inventories,
                AccountsReceivable = request.AccountsReceivable,
                CashAndEquivalents = request.CashAndEquivalents,
                NonCurrentLiabilities = request.NonCurrentLiabilities,
                CurrentLiabilities = request.CurrentLiabilities
            };

            return CreatedAtRoute("GetFinancialReportById", new { id = createdFinancialReportId }, financialReportModel);
        }

        /// <summary>
        /// Обновить информацию о финансовом отчете
        /// </summary>
        /// <param name="request"> Обновленная карточка финансового отчета. </param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EditFinancialReportAsync([FromBody] UpdatingFinancialReportModel request)
        {
            var dto = FinancialReportMapper.ToDto(request);
            if (dto == null)
                return BadRequest("Запрос пустой");
            await _financialReportService.UpdateAsync(dto);
            return Ok();
        }

        /// <summary>
        /// Удалить финансовый отчет
        /// </summary>
        /// <param name="id"> Id финансового отчета</param>
        /// <returns></returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFinancialReport(Guid id)
        {
            await _financialReportService.DeleteAsync(id);
            return NoContent();
        }
    }
}
