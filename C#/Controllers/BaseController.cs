using HKSportsServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace HKSportsServer.Controllers
{
    public partial class HKSportsController : Controller
    {

        // Error 처리
        private HKRheader setHKRheader_Err(HKRheader header, int noErr, 
                                    string msgErr, string token, LOGDBDataContext logdb, LOG Log)
        {
            header.auth_token = token;
            // 결과에 대한 result_code 처리 로직 추가 필요
            header.result_code = noErr;
            header.message = msgErr;

            Log.result = msgErr;
            logdb.LOGs.InsertOnSubmit(Log);
            logdb.SubmitChanges();

            return header;
        }



        private void AppendLog(string log)
        {
            string path = @"C:\logs\hkrider"+ DateTime.Now.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture) + ".log";

            if (!System.IO.File.Exists(path)) System.IO.File.Create(path);

            using (FileStream stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None))
            {
                using (var tw = new StreamWriter(stream))
                {
                    tw.WriteLineAsync(DateTime.Now.ToString() + ": " + log);
                    tw.Close();
                }
            }

        }

        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding,
                                                                JsonRequestBehavior behavior)
        {
            return new JsonNetResult(data, contentType, contentEncoding, behavior);
        }


        // Firebase Cloud Messaging 보내기.
        private void sendFCM_lostbike(LOSTBIKE lostBike, string lost_found, PUSHRECORD pushRecord)
        {
            string serverKey = "AAAAGb-LKYs:APA91bEuJrpmtMaFicGReVqpinlj-Mzk8qniXLhFkqcCtJVLJ9h7LoPFHK7wDTUlhx7OAhqGgSONUVYvU5BUQey22CegTMWAhzKp3DCiEbjU67fGDaTWt88J2vvuA3WgemufF_OZ4J_6";

            try
            {
                var result = "-1";
                var webAddr = "https://fcm.googleapis.com/fcm/send";

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Headers.Add("Authorization:key=" + serverKey);
                httpWebRequest.Method = "POST";

                FCMdata fcmData = new FCMdata();
                fcmData.to = "/topics/lostbikes";
                FCMdataBody fcmBody = new FCMdataBody();
                if (lost_found.Equals("L"))
                {
                    fcmBody.msg_type = "LOST";
                    fcmBody.title = "분실도움 요청";
                    fcmBody.msg = lostBike.UserName + " 님이 " + lostBike.BikeNickname +
                                    "의 위치를 찾기 위해 분실도움요청을 하셨습니다.";
                } else
                {
                    fcmBody.msg_type = "LOSTCANCEL";
                    fcmBody.title = "분실도움 취소";
                    fcmBody.msg = lostBike.UserName + " 님이 " + lostBike.BikeNickname +
                                    "의 분실신고를 취소하셨습니다.";
                }
                fcmBody.data = lostBike.UserName + "_" + lostBike.BikeNickname + "_" + 
                                lostBike.DeviceID;
                fcmData.data = fcmBody;

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(fcmData);
                    //"{\"to\": \"/topics/lostbikes\",\"data\": {\"message\": \"This is a Firebase Cloud Messaging Topic Message!\",}}";
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    pushRecord.request = json;
                }


                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                    pushRecord.response = result.ToString();
                  
                }

            }
            catch (Exception ex)
            {
                pushRecord.error = ex.Message;
                //  Response.Write(ex.Message);
            }
        }


    }
}