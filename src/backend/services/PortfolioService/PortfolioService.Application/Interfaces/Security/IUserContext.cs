namespace StockMarketAssistant.PortfolioService.Application.Interfaces.Security
{
    /// <summary>
    /// Контекст текущего пользователя
    /// </summary>
    public interface IUserContext
    {
        /// <summary>
        /// ID текущего пользователя из JWT
        /// </summary>
        Guid UserId { get; }

        /// <summary>
        /// Email текущего пользователя из JWT
        /// </summary>
        string Email { get; }

        /// <summary>
        /// Текущая роль
        /// </summary>
        string Role { get; }

        /// <summary>
        /// Является ли пользователь админом?
        /// </summary>
        bool IsAdmin => Role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
    }

}
