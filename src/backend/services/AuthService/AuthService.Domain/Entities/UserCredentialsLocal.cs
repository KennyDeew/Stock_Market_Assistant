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
        public User User { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string PasswordHash { get; set; }
        public DateTimeOffset LastPasswordChange { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
