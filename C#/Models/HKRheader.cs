using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HKSportsServer.Models
{
    public class HKRheader
    {
        public string action { get; set; }
        public string auth_token { get; set; }
        public int result_code { get; set; } //OK:0,Update required:1, Error:100~
        public string message { get; set; } // messages for Error etc
        public string app_key { get; set; }
        public string auth_device_id { get; set; }
        public string client_version { get; set; }
        public string client_os { get; set; }
        public string client_market { get; set; }

        public int mailbox_idx { get; set; } //메일박스가 어디까지 쌓여 있는지 번호
        public string ret_string { get; set; } // 상황에 따라서 추가된 로그의 ID를 돌려 받거나 할 수 있다. action에 따라서 정의가 된다.

    }
}