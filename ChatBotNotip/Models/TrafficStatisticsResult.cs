using System;
using System.Collections.Generic;

namespace ChatBotNotip.Models
{
    public partial class TrafficStatisticsResult
    {
        public int? Year { get; set; }
        public int Month { get; set; }
        public int? Day { get; set; }
        public int MessageCount { get; set; }
        public int LoginCount { get; set; }
    }
}
