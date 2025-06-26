using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarketAssistant.AuthService.Domain.Entities
{
    public class UserRefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public string TokenHash { get; set; }
        public string Fingerprint { get; set; }
        public string UserAgent { get; set; }
        public DateTimeOffset IssuedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public bool Revoked { get; set; }
    }
}
