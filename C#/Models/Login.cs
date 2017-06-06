using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HKSportsServer.Models
{
    [Serializable]
    public class Login
    {
        public login_header header { get; set; }
        public login_body body { get; set; }
    }

    [Serializable]
    public class login_header
    {
        public string action        { get; set; }
        public string app_key       { get; set; }
        public string auth_token    { get; set; }
        public string auth_device_id { get; set; }
        public string client_version { get; set; }
        public string client_os     { get; set; }
        public string client_market { get; set; }
    }

    [Serializable]
    public class login_body
    {
        public string email         { get; set; }
        public string password      { get; set; }
    }
}