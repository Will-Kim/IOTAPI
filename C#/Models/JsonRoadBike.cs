using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HKSportsServer.Models
{
    public class JsonRoadBike
    {
        public HKRheader    header  { get; set; }
        public RoadBike     body    { get; set; }
    }
}