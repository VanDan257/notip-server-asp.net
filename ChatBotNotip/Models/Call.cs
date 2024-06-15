using System;
using System.Collections.Generic;

namespace ChatBotNotip.Models
{
    public partial class Call
    {
        public int Id { get; set; }
        public Guid GroupCallCode { get; set; }
        public Guid UserCode { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
        public DateTime Created { get; set; }

        public virtual GroupCall GroupCallCodeNavigation { get; set; }
        public virtual User UserCodeNavigation { get; set; }
    }
}
