using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HKSportsServer.Models
{
    public class RoadBike
    {
        public int? id { get; set; }
        public int user_id { get; set; }
        public string nickname { get; set; }
        public string device_id { get; set; }
        public int? type { get; set; }
        public string typename { get; set; }
        public string wheel_size { get; set; }
        public string modelname { get; set; }
        public string color_size { get; set; }
        public string frame_no { get; set; }
        public string characteristics { get; set; }
        public string pictures { get; set; }
        public int? avatar_level { get; set; }
        public int? riding_score { get; set; }
        public int? riding_count { get; set; }
        public int? distance_accumulated { get; set; }
        public int? time_accumulated { get; set; }
        public int? average_distance { get; set; }
        public int? average_riding_time { get; set; }
        public int? calories_accumulated { get; set; }
        public int? average_calories { get; set; }
        public string lost { get; set; }
        public string last_gps { get; set; }
    }
}