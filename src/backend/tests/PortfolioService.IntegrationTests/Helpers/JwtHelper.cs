using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StockMarketAssistant.PortfolioService.IntegrationTests.Helpers
{
    public static class JwtHelper
    {
        public static string GenerateTestToken(string userId, string role)
        {
            var claims = new[]
            {
                new Claim("Id", userId),
                new Claim("Role", role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TestKeyVeryLongAndSecureEnoughForTests12345"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "TestIssuer",
                audience: "TestAudience",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
