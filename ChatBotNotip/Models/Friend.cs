using System;
using System.Collections.Generic;

namespace ChatBotNotip.Models
{
    public partial class Friend
    {
        public int Id { get; set; }
        public Guid SenderCode { get; set; }
        public Guid ReceiverCode { get; set; }
        public string Status { get; set; }
    }
}
