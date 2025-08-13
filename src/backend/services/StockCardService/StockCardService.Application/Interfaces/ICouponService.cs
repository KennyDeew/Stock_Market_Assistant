using StockMarketAssistant.StockCardService.Application.DTOs._02sub_Coupon;

namespace StockMarketAssistant.StockCardService.Application.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса по работе с купонами облигации
    /// </summary>
    public interface ICouponService
    {
        /// <summary>
        /// Получить список всех купонов всех облигаций
        /// </summary>
        /// <returns>Список всех купонов всех облигаций</returns>
        Task<IEnumerable<CouponDto>> GetAllAsync();

        /// <summary>
        /// Получить список всех купонов одной облигации
        /// </summary>
        /// <param name="id">Id облигации</param>
        /// <returns>Список купонов указанной облигации</returns>
        Task<IEnumerable<CouponDto>> GetAllByParentIdAsync(Guid id);

        /// <summary>
        /// Получить карточку купона по Id
        /// </summary>
        /// <param name="id">Идентификатор купона облигации</param>
        /// <returns>DTO купона облигации</returns>
        Task<CouponDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Создать купона
        /// </summary>
        /// <param name="creatingCouponDto">DTO создаваемого купона облигации</param>
        Task<Guid> CreateAsync(CreatingCouponDto creatingCouponDto);

        /// <summary>
        /// Обновить купон
        /// </summary>
        /// <param name="updatingCoupondDto">DTO редактируемого купона облигации</param>
        Task UpdateAsync(UpdatingCouponDto updatingCoupondDto);

        /// <summary>
        /// Удалить купон
        /// </summary>
        /// <param name="id">Идентификатор купона</param>
        Task DeleteAsync(Guid id);
    }
}
