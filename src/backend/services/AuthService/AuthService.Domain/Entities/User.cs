using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarketAssistant.AuthService.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset LastActiveAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
        public UserCredentialsLocal CredentialsLocal { get; set; }
        public ICollection<UserRefreshToken> RefreshTokens { get; set; }
    }
}
