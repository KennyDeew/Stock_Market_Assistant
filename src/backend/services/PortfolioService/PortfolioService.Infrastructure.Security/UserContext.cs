using Microsoft.AspNetCore.Http;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Security;
using StockMarketAssistant.PortfolioService.Domain.Exceptions;

namespace StockMarketAssistant.PortfolioService.Infrastructure.Security
{
    public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public Guid UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("Id")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    throw new SecurityException("Требуется авторизация");
                return userId;
            }
        }

        public string Role
        {
            get
            {
                var roleClaim = _httpContextAccessor.HttpContext?.User.FindFirst("Role")?.Value;
                return roleClaim ?? "UNKNOWN";
            }
        }
    }
}
