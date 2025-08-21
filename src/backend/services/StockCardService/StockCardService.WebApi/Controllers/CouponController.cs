using Microsoft.AspNetCore.Mvc;
using StockMarketAssistant.StockCardService.WebApi.Models._02sub_Coupon;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.WebApi.Mappers;

namespace StockCardService.WebApi.Controllers
{
    /// <summary>
    /// Купоны
    /// </summary>
    [Route("api/v1/[controller]")]
    public class CouponController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        /// <summary>
        /// Получить все купоны всех акций
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<CouponModel>>> GetAllCouponsAsync()
        {
            var coupons = (await _couponService.GetAllAsync()).Select(CouponMapper.ToModel).ToList();
            return coupons;
        }

        /// <summary>
        /// Получить все купоны определенной акции
        /// </summary>
        /// <returns></returns>
        [HttpGet("ByBondCard/{bondCardId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CouponModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<ActionResult<List<CouponModel>>> GetAllCouponsByParentIdAsync(Guid bondCardId)
        {
            var coupons = (await _couponService.GetAllByParentIdAsync(bondCardId)).Select(CouponMapper.ToModel).ToList();
            return coupons;
        }

        /// <summary>
        /// Получить купон по Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CouponModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<ActionResult<CouponModel>> GetCouponByIdAsync(Guid id)
        {
            var couponDto = await _couponService.GetByIdAsync(id);

            if (couponDto == null)
                return NotFound();

            var couponModel = CouponMapper.ToModel(couponDto);
            return couponModel;
        }

        /// <summary>
        /// Добавить новый купон
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(CouponModel), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateCoupon(CreatingCouponModel request)
        {
            var createdCouponId = await _couponService.CreateAsync(CouponMapper.ToDto(request));
            if (createdCouponId == Guid.Empty) return Problem("Не удалось создать клиента");

            var couponModel = new CouponModel()
            {
                Id = createdCouponId,
                BondId = request.BondId,
                Currency = request.Currency,
                CutOffDate = request.CutOffDate,
                Value = request.Value
            };

            return CreatedAtRoute("GetCouponModel", new { id = createdCouponId }, couponModel);
        }

        /// <summary>
        /// Получить модель купона
        /// </summary>
        /// <returns></returns>
        [HttpGet("Model/{id:guid}", Name = "GetCouponModel")]
        [ProducesResponseType(typeof(CouponModel), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<CouponModel>> GetCouponModelAsync(Guid id)
        {
            var couponDto = await _couponService.GetByIdAsync(id);

            if (couponDto == null)
                return NotFound();

            var couponModel = CouponMapper.ToModel(couponDto);
            return couponModel;
        }

        /// <summary>
        /// Обновить информацию о купоне
        /// </summary>
        /// <param name="request"> Обновленная карточка купона. </param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> EditCouponAsync(UpdatingCouponModel request)
        {
            await _couponService.UpdateAsync(CouponMapper.ToDto(request));
            return Ok();
        }

        /// <summary>
        /// Удалить купон
        /// </summary>
        /// <param name="id"> Id купона</param>
        /// <returns></returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteCoupon(Guid id)
        {
            await _couponService.DeleteAsync(id);
            return NoContent();
        }
    }
}
