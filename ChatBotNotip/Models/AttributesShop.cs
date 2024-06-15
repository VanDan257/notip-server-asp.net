using System;
using System.Collections.Generic;

namespace ChatBotNotip.Models
{
    public partial class AttributesShop
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public Guid ShopId { get; set; }

        public virtual Shop Shop { get; set; }
    }
}
