using StockCardService.Abstractions.Repositories;
using StockMarketAssistant.StockCardService.Application.DTOs._02sub_Coupon;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockMarketAssistant.StockCardService.Application.Services
{
    /// <summary>
    /// Сервис работы с купонами облигации
    /// </summary>
    public class CouponService : ICouponService
    {
        private readonly ISubRepository<Coupon, Guid> _couponRepository;

        public CouponService(ISubRepository<Coupon, Guid> couponRepository)
        {
            _couponRepository = couponRepository;
        }

        /// <summary>
        /// Получить все купоны всех облигаций
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<CouponDto>> GetAllAsync()
        {
            var coupons = await _couponRepository.GetAllAsync(CancellationToken.None);
            var couponDtoList = coupons.Select(x =>
                new CouponDto()
                {
                    Id = x.Id,
                    BondId = x.ParentId,
                    CutOffDate = x.CuttOffDate,
                    Currency = x.Currency,
                    Value = x.Value
                }).ToList();

            return couponDtoList;
        }

        /// <summary>
        /// Получить список всех купонов одной облигации
        /// </summary>
        /// <param name="id">Id облигации</param>
        /// <returns>Список купонов указанной облигации</returns>
        public async Task<IEnumerable<CouponDto>> GetAllByParentIdAsync(Guid id)
        {
            var coupons = await _couponRepository.GetAllByParentIdAsync(id, CancellationToken.None);
            var couponDtoList = coupons.Select(x =>
                new CouponDto()
                {
                    Id = x.Id,
                    BondId = x.ParentId,
                    CutOffDate = x.CuttOffDate,
                    Currency = x.Currency,
                    Value = x.Value
                }).ToList();

            return couponDtoList;
        }

        /// <summary>
        /// Получить купон по Id
        /// </summary>
        /// <param name="id">ID купона</param>
        /// <returns></returns>
        public async Task<CouponDto?> GetByIdAsync(Guid id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id, CancellationToken.None);

            if (coupon == null)
                return null;

            var couponDto = new CouponDto()
            {
                Id = coupon.Id,
                BondId = coupon.ParentId,
                CutOffDate = coupon.CuttOffDate,
                Currency = coupon.Currency,
                Value = coupon.Value
            };
            return couponDto;
        }

        /// <summary>
        /// Добавить новый купон
        /// </summary>
        /// <param name="creatingCouponDto">Dto создаваемого купона</param>
        /// <returns></returns>
        public async Task<Guid> CreateAsync(CreatingCouponDto creatingCouponDto)
        {
            var сoupon = new Coupon()
            {
                Id = Guid.NewGuid(),
                ParentId = creatingCouponDto.BondId,
                CuttOffDate = creatingCouponDto.CutOffDate,
                Currency = creatingCouponDto.Currency,
                Value = creatingCouponDto.Value
            };

            var createdCoupon = await _couponRepository.AddAsync(сoupon);
            return createdCoupon.Id;
        }

        /// <summary>
        /// Обновить информацию о купоне облигации
        /// </summary>
        /// <param name="updatingCouponDto">Dto измененненного купона</param>
        /// <returns></returns>
        public async Task UpdateAsync(UpdatingCouponDto updatingCouponDto)
        {
            var coupon = await _couponRepository.GetByIdAsync(updatingCouponDto.Id, CancellationToken.None);
            if (coupon == null)
                return;
            coupon.CuttOffDate = updatingCouponDto.CutOffDate;
            coupon.Value = updatingCouponDto.Value;
            await _couponRepository.UpdateAsync(coupon);
        }

        /// <summary>
        /// Удалить купон
        /// </summary>
        /// <param name="id">Id купона</param>
        /// <returns></returns>
        public async Task DeleteAsync(Guid id)
        {
            await _couponRepository.DeleteAsync(id);
        }
    }
}
