using StockMarketAssistant.StockCardService.Application.DTOs._02sub_Coupon;
using StockMarketAssistant.StockCardService.WebApi.Models._02sub_Coupon;

namespace StockMarketAssistant.StockCardService.WebApi.Mappers
{
    /// <summary>
    /// Маппер для купонов облигаций model/dto.
    /// </summary>
    public class CouponMapper
    {
        /// <summary>
        /// Конвертирует модель купона облигации в DTO.
        /// </summary>
        /// <param name="model">Исходная модель купона облигации.</param>
        /// <returns>DTO купона облигации.</returns>
        public static CouponDto ToDto(CouponModel model)
        {
            return new CouponDto
            {
                Id = model.Id,
                BondId = model.BondId,
                CutOffDate = DateTime.SpecifyKind(model.CutOffDate, DateTimeKind.Utc),
                Currency = model.Currency,
                Value = model.Value
            };
        }

        /// <summary>
        /// Конвертирует модель создаваемого купона облигации в DTO.
        /// </summary>
        /// <param name="model">Исходная модель создаваемого купона облигации.</param>
        /// <returns>DTO создаваемого купона облигации.</returns>
        public static CreatingCouponDto ToDto(CreatingCouponModel model)
        {
            return new CreatingCouponDto
            {
                BondId = model.BondId,
                CutOffDate = DateTime.SpecifyKind(model.CutOffDate, DateTimeKind.Utc),
                Currency = model.Currency,
                Value = model.Value
            };
        }

        /// <summary>
        /// Конвертирует модель изменяемого купона облигации в DTO.
        /// </summary>
        /// <param name="model">Исходная модель изменяемого купона облигации.</param>
        /// <returns>DTO изменяемого купона облигации.</returns>
        public static UpdatingCouponDto ToDto(UpdatingCouponModel model)
        {
            return new UpdatingCouponDto
            {
                CutOffDate = DateTime.SpecifyKind(model.CutOffDate, DateTimeKind.Utc),
                Value = model.Value
            };
        }

        /// <summary>
        /// Конвертирует DTO купона облигации в модель.
        /// </summary>
        /// <param name="dto">Исходный DTO купона облигации.</param>
        /// <returns>Модель купона облигации.</returns>
        public static CouponModel ToModel(CouponDto dto)
        {
            return new CouponModel
            {
                Id = dto.Id,
                BondId = dto.BondId,
                CutOffDate = dto.CutOffDate,
                Currency = dto.Currency,
                Value = dto.Value
            };
        }

        /// <summary>
        /// Конвертирует DTO создаваемого купона облигации в модель.
        /// </summary>
        /// <param name="dto">Исходный DTO создаваемого купона облигации.</param>
        /// <returns>Модель создаваемого купона облигации.</returns>
        public static CreatingCouponModel ToDto(CreatingCouponDto dto)
        {
            return new CreatingCouponModel
            {
                BondId = dto.BondId,
                CutOffDate = dto.CutOffDate,
                Currency = dto.Currency,
                Value = dto.Value
            };
        }

        /// <summary>
        /// Конвертирует DTO изменяемого купона облигации в модель.
        /// </summary>
        /// <param name="dto">Исходный DTO изменяемого купона облигации.</param>
        /// <returns>Модель изменяемого купона облигации.</returns>
        public static UpdatingCouponModel ToDto(UpdatingCouponDto dto)
        {
            return new UpdatingCouponModel
            {
                CutOffDate = dto.CutOffDate,
                Value = dto.Value
            };
        }
    }
}