using HKSportsServer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;


namespace HKSportsServer.Controllers
{
    public partial class HKSportsController : Controller
    {
        // POST: RemoveBikeLost
        [System.Web.Http.HttpPost]
        public ActionResult ReportBikeLost(int? id)
        {
            Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();
            //AppendLog("[REQ HKSports/LoginController]" + json);

            string actionName = "ReportBikeLost";
            DateTime dtNow = DateTime.Now;
     
            // 객체 초기화
            JsonBikeLost jsonBikeLost = null;
            UserInfo returnUserInfo = new UserInfo();
            HKRheader rHeader = new HKRheader();
            UserInfoBody uiBody = new UserInfoBody();

            LOG newLog = new LOG(); // 로그 객체

            // DB Context 가져오기
            HKRiderDBDataContext db = new HKRiderDBDataContext(/*connectionString here */);
            LOGDBDataContext logdb = new LOGDBDataContext(/**/);
            try
            {
                jsonBikeLost = JsonConvert.DeserializeObject<JsonBikeLost>(json);

                // Log 처리 루틴 ------------------------------------------------- 
                newLog.action = jsonBikeLost.header.action;
                newLog.auth_token = jsonBikeLost.header.auth_token;
                newLog.json = json;
                newLog.dt_created = dtNow;
                newLog.user_id = jsonBikeLost.body.nickname;
                // Log 처리 루틴 ------------------------------------------------- 
            
                if (!jsonBikeLost.header.action.Equals(actionName))
                {
                    returnUserInfo.header = setHKRheader_Err(rHeader, 101, "[ERROR] Action is wrong: " + jsonBikeLost.header.action,
                                            jsonBikeLost.header.auth_token, logdb, newLog);
                    return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
                } else if (jsonBikeLost.header.auth_token == null || jsonBikeLost.header.auth_token == "")
                {
                    returnUserInfo.header = setHKRheader_Err(rHeader, 103, "[ERROR] Auth Token is wrong: " + jsonBikeLost.header.auth_token,
                                            jsonBikeLost.header.auth_token, logdb, newLog);
                    return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
                }

                // Version Check 추가 예정

                // User 가져오기
                USER thisUser = (   from    u in db.USERs
                                    where   u.Auth_Token == jsonBikeLost.header.auth_token
                                    select  u).SingleOrDefault();

                if (thisUser == null) // 유저가 존재하는지 확인
                {
                    //Error 202: 다시 접속하세요. 유저정보를 확인할 수 없습니다.
                    returnUserInfo.header = setHKRheader_Err(rHeader, 202, "[ERROR] 다시 접속하세요. 유저정보를 확인할 수 없습니다: " +
                                thisUser.EMail, thisUser.Auth_Token, logdb, newLog);
                    return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
                }
                else // 분실신고 바이크의 정보가 있는지 확인.
                {
                    ROADBIKE theBike = db.ROADBIKEs.Where(r => r.Nickname == jsonBikeLost.body.nickname).SingleOrDefault();
                      
                    if (theBike == null) //존재하지 않는 바이크.
                    {
                        //Error 205: 아바타 정보를 확인할 수 없습니다. 계속 문제 발생시 다시 로그인해 주세요.
                        returnUserInfo.header = setHKRheader_Err(rHeader, 205, "[ERROR] 아바타 정보를 확인할 수 없습니다. 계속 문제 발생시 다시 로그인해 주세요.: " +
                                    thisUser.EMail, thisUser.Auth_Token, logdb, newLog);
                        return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
                    }
                    else // 분실 신고 테이블에 등록
                    {
                        USER theUser = db.USERs.Where(u => u.id == theBike.USERS_id).SingleOrDefault();
                        if (theUser == null) // 유저가 존재하는지 확인
                        {
                            //Error 202: 다시 접속하세요. 유저정보를 확인할 수 없습니다.
                            returnUserInfo.header = setHKRheader_Err(rHeader, 202, "[ERROR] 다시 접속하세요. 유저정보를 확인할 수 없습니다: " +
                                        theUser.EMail, thisUser.Auth_Token, logdb, newLog);
                            return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
                        }
   
                        LOSTBIKE lostBike = (from l in db.LOSTBIKEs
                                             where l.BikeNickname == jsonBikeLost.body.nickname
                                                   && l.DeviceID == jsonBikeLost.body.device_id
                                                   && l.Status.Contains("L")
                                             select l).SingleOrDefault();
                        if (lostBike == null) //존재하지 않는 분실신고.
                        {
                            //Error 207: 취소할 분실신고가 없습니다.
                            returnUserInfo.header = setHKRheader_Err(rHeader, 207, "[ERROR] 취소할 분실신고가 없습니다.: " +
                                        thisUser.EMail, thisUser.Auth_Token, logdb, newLog);
                            return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
                        } else
                        {
                            theBike.LastGPS = jsonBikeLost.body.dt_lost+":"+jsonBikeLost.body.location_lost;

                            PUSHRECORD pushRecord = new PUSHRECORD();
                            pushRecord.dt = dtNow;

                            sendFCM_lostbike_found(theUser, jsonBikeLost.body, pushRecord);

                            db.PUSHRECORDs.InsertOnSubmit(pushRecord);
                        }
                    }
                }


                db.SubmitChanges();
                logdb.LOGs.InsertOnSubmit(newLog);

                returnUserInfo.header = rHeader;
                returnUserInfo.body = uiBody;
                returnUserInfo = getUserInformation(returnUserInfo, thisUser); // 다시 계정 정보를 받아서 리턴해 준다.

                return Json(returnUserInfo, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                // Error: Exception
                returnUserInfo.header = setHKRheader_Err(rHeader, 301, "[ERROR] Exception: " + ex.Message
                    , jsonBikeLost==null?"jsonRoadBike is null": jsonBikeLost.header.auth_token, logdb, newLog);
                return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
            }

        }

        // Firebase Cloud Messaging 보내기.
        private void sendFCM_lostbike_found(USER theUser, BikeLost bikeLost, PUSHRECORD pushRecord)
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
                fcmData.to = theUser.FCM_Token;
                FCMdataBody fcmBody = new FCMdataBody();

                fcmBody.msg_type = "FOUND";
                fcmBody.title = "자전거 발견";
                fcmBody.msg = bikeLost.nickname + "의 " + 
                                "최근 위치가 감지되었습니다.";
                fcmBody.data = bikeLost.nickname + "_" + bikeLost.location_lost + "_" +
                                bikeLost.dt_lost;
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