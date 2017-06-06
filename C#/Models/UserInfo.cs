using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HKSportsServer.Models
{
    public class UserInfo
    {
        public HKRheader header { get; set; }
        public UserInfoBody body { get; set; }
    }

    public class UserInfoBody
    {
        public string email { get; set; }
        public string password { get; set; }
        public string   sex       { get; set; }
        public int      year_born { get; set; }
        public int      height    { get; set; }
        public int      weight    { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<RoadBike> road_bikes { get; set; }
    }
}