using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarketAssistant.AuthService.Domain.Entities
{
    public class UserCredentialsLocal
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = default!;
        public string Nickname { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public DateTime LastPasswordChange { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        public User User { get; set; } = default!;
    }
}
