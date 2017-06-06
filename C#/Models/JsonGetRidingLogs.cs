using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HKSportsServer.Models
{
    public class JsonGetRidingLogs
    {
        public HKRheader header { get; set; }
        public RequestLogs body { get; set; }

        public class RequestLogs
        {
            public long from_date { get; set; }
            public long to_date { get; set; }
            public string mac_address { get; set; }
            public long remove_id { get; set; }
        }
    }
}