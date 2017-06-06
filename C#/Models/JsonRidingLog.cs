using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HKSportsServer.Models
{
    public class JsonRidingLog
    {
        public HKRheader    header  { get; set; }
        public RidingLog    body    { get; set; }
    }
}