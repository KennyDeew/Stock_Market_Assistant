using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarketAssistant.AuthService.Domain.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public string Resource { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; }
    }
}
