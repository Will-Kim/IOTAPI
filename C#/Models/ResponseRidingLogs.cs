using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HKSportsServer.Models
{
    public class ResponseRidingLogs
    {
        public HKRheader header;
        public List<RidingLog> body;
    }
}