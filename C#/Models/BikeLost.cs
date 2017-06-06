using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HKSportsServer.Models
{
    public class BikeLost
    {
        public int id {get; set;}
        public string username {get; set;}
        public string nickname {get; set;}
        public string phone_no {get; set;}
        public string extra_info {get; set;}
        public string modelname {get; set;}
        public string color_size {get; set;}
        public string frame_no {get; set;}
        public string characteristics {get; set;}

        public string location_lost {get; set;}
        public string status {get; set; }            // L:losted, F:found, C:canceled
        public long dt_lost {get; set;}

        public string device_id {get; set;} // 블루투스 맥어드레스
    }
}