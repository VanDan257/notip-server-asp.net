using System;
using System.Collections.Generic;

namespace ChatBotNotip.Models
{
    public partial class Shop
    {
        public Shop()
        {
            AttributesShops = new HashSet<AttributesShop>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public string Description { get; set; }
        public int Start { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual ICollection<AttributesShop> AttributesShops { get; set; }
    }
}
