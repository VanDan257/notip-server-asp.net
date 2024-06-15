using System;
using System.Collections.Generic;

namespace ChatBotNotip.Models
{
    public partial class AspNetRole
    {
        public AspNetRole()
        {
            RoleClaims = new HashSet<RoleClaim>();
            Users = new HashSet<User>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string ConcurrencyStamp { get; set; }

        public virtual ICollection<RoleClaim> RoleClaims { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
