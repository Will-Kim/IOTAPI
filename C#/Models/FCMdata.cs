using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HKSportsServer.Models
{
    public class FCMdata
    {
        public string to { get; set; }
        public FCMdataBody data { get; set; }
    }

    public class FCMdataBody
    {
        public string title { get; set; }
        public string msg_type { get; set; }        // LOST, LOSTCANCEL, ...
        public string msg { get; set; }
        public string data { get; set; }
    }
}