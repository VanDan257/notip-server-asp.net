using System;
using System.Collections.Generic;

namespace ChatBotNotip.Models
{
    public partial class GroupUser
    {
        public long Id { get; set; }
        public Guid GroupCode { get; set; }
        public Guid UserCode { get; set; }

        public virtual Group GroupCodeNavigation { get; set; }
        public virtual User UserCodeNavigation { get; set; }
    }
}
