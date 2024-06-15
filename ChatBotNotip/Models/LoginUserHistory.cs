using System;
using System.Collections.Generic;

namespace ChatBotNotip.Models
{
    public partial class LoginUserHistory
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime LoginTime { get; set; }

        public virtual User User { get; set; }
    }
}
