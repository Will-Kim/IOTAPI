using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HKSportsServer.Models
{
    public class JsonBikeLost
    {
        public HKRheader    header  { get; set; }
        public BikeLost     body    { get; set; }
    }
}