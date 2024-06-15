using System;
using System.Collections.Generic;

namespace ChatBotNotip.Models
{
    public partial class Group
    {
        public Group()
        {
            GroupUsers = new HashSet<GroupUser>();
            Messages = new HashSet<Message>();
        }

        public Guid Code { get; set; }
        /// <summary>
        /// single: chat 1-1
        /// multi: chat 1-n
        /// </summary>
        public string Type { get; set; }
        public string Avatar { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime LastActive { get; set; }
        public int TypeChatId { get; set; }

        public virtual ICollection<GroupUser> GroupUsers { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}
