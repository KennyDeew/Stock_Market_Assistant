namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    /// <summary>
    /// Модель ответа с пагинацией
    /// </summary>
    /// <typeparam name="T">Тип элементов</typeparam>
    /// <param name="Items">Список элементов</param>
    /// <param name="TotalCount">Общее количество элементов</param>
    /// <param name="Page">Номер текущей страницы</param>
    /// <param name="PageSize">Размер страницы</param>
    /// <param name="TotalPages">Общее количество страниц</param>
    public record PaginatedResponse<T>(
        IEnumerable<T> Items,
        int TotalCount,
        int Page,
        int PageSize,
        int TotalPages)
    {
        /// <summary>
        /// Существует ли предыдущая страница
        /// </summary>
        public bool HasPreviousPage => Page > 1;

        /// <summary>
        /// Существует ли следующая страница
        /// </summary>
        public bool HasNextPage => Page < TotalPages;
    }
}
