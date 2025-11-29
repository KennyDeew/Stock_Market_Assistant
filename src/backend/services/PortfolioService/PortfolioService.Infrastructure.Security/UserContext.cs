using Microsoft.AspNetCore.Http;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Security;

namespace StockMarketAssistant.PortfolioService.Infrastructure.Security
{
    public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public Guid UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User.FindFirst("Id")?.Value;
                return Guid.TryParse(userIdClaim, out var id) ? id : Guid.Empty;
            }
        }

        public string Email => _httpContextAccessor.HttpContext?
            .User.FindFirst("Email")?.Value ?? string.Empty;


        public string Role => _httpContextAccessor.HttpContext?
            .User.FindFirst("Role")?.Value ?? "UNKNOWN";
    }
}
