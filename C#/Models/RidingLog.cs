using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HKSportsServer.Models
{
    public class RidingLog
    {
        public long id { get; set; }
        public int? riding_score { get; set; }
        public int? distance { get; set; } // meter
        public long? duration { get; set; } //
        public long? dt_start { get; set; }
        public long? dt_end { get; set; }
        public int? average_speed { get; set; } // m/sec
        public int? max_speed { get; set; }
        public int? calories_burn { get; set; }
        public string gps_records { get; set; }
        public string start_location { get; set; }
        public string end_location { get; set; }
        public string mac_address { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string debug_data { get; set; }

        public string last_gps { get; set; }
    }
}