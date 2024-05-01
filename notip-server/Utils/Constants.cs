using System;
using static System.Net.WebRequestMethods;

namespace notip_server.Utils
{
    public class Constants
    {
        public class GroupType
        {
            public const string SINGLE = "single";
            public const string MULTI = "multi";
        }

        public class CallStatus
        {
            public const string IN_COMMING = "IN_COMMING";
            public const string OUT_GOING = "OUT_GOING";
            public const string MISSED = "MISSED";
        }

        public const string AVATAR_DEFAULT = "images/no_image.jpg";
    }
}
