using System;
using System.Collections.Generic;

namespace ChatBotNotip.Models
{
    public partial class Message
    {
        public long Id { get; set; }
        /// <summary>
        /// text
        /// media
        /// attachment
        /// </summary>
        public string Type { get; set; }
        public Guid GroupCode { get; set; }
        public string Content { get; set; }
        public string Path { get; set; }
        public DateTime Created { get; set; }
        public Guid CreatedBy { get; set; }

        public virtual User CreatedByNavigation { get; set; }
        public virtual Group GroupCodeNavigation { get; set; }
    }
}
