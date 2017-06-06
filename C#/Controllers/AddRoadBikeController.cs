using HKSportsServer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;


namespace HKSportsServer.Controllers
{
    public partial class HKSportsController : Controller
    {
        [System.Web.Http.HttpPost]
        public ActionResult AddRoadBike(int? id)
        {
            Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();
            //AppendLog("[REQ HKSports/LoginController]" + json);

            string actionName = "AddRoadBike";
            DateTime dtNow = DateTime.Now;

            // 객체 초기화
            JsonRoadBike jsonRoadBike = null;
            UserInfo returnUserInfo = new UserInfo();
            HKRheader rHeader = new HKRheader();
            UserInfoBody uiBody = new UserInfoBody();

            LOG newLog = new LOG(); // 로그 객체

            // DB Context 가져오기
            HKRiderDBDataContext db = new HKRiderDBDataContext(/*connectionString here */);
            LOGDBDataContext logdb = new LOGDBDataContext(/**/);
            try
            {
                jsonRoadBike = JsonConvert.DeserializeObject<JsonRoadBike>(json);

                // Log 처리 루틴 -------------------------------------------------
                newLog.action = jsonRoadBike.header.action;
                newLog.auth_token = jsonRoadBike.header.auth_token;
                newLog.json = json;
                newLog.dt_created = dtNow;
                newLog.user_id = jsonRoadBike.body.nickname;
                // Log 처리 루틴 -------------------------------------------------

                if (!jsonRoadBike.header.action.Equals(actionName))
                {
                    returnUserInfo.header = setHKRheader_Err(rHeader, 101, "[ERROR] Action is wrong: " + jsonRoadBike.header.action,
                                            returnUserInfo.header.auth_token, logdb, newLog);
                    return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
                } else if (jsonRoadBike.header.auth_token == null || jsonRoadBike.header.auth_token == "")
                {
                    returnUserInfo.header = setHKRheader_Err(rHeader, 103, "[ERROR] Auth Token is wrong: " + jsonRoadBike.header.auth_token,
                                            returnUserInfo.header.auth_token, logdb, newLog);
                    return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
                }

                // Version Check 추가 예정

                // User 가져오기
                USER thisUser = (   from    u in db.USERs
                                    where   u.Auth_Token == jsonRoadBike.header.auth_token
                                    select  u).SingleOrDefault();

                if (thisUser == null)
                {
                    //Error 202: 다시 접속하세요. 유저정보를 확인할 수 없습니다.
                    returnUserInfo.header = setHKRheader_Err(rHeader, 202, "[ERROR] 다시 접속하세요. 유저정보를 확인할 수 없습니다: " +
                                jsonRoadBike.header.auth_token, jsonRoadBike.header.auth_token, logdb, newLog);
                    return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ROADBIKE theBike = thisUser.ROADBIKEs.Where(r => r.Nickname == jsonRoadBike.body.nickname).SingleOrDefault();

                    if (theBike == null) //신규 바이크.
                    {
                        foreach(ROADBIKE rb in thisUser.ROADBIKEs)
                        {
                            if(rb.DeviceID.Equals(jsonRoadBike.body.device_id))
                            {
                                //Error 204: 이미 존재하는 디바이스 ID입니다.
                                returnUserInfo.header = setHKRheader_Err(rHeader, 204,
                                            "[ERROR] 이미 존재하는 디바이스 ID입니다: " + jsonRoadBike.body.device_id + "," +
                                            thisUser.EMail, thisUser.Auth_Token, logdb, newLog);
                                return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
                            }
                        }
                        ROADBIKE newRoadBike = new ROADBIKE();
                        newRoadBike.Nickname = jsonRoadBike.body.nickname;
                        newRoadBike.Type = (byte)jsonRoadBike.body.type; // 타입을 정수로 보관할 필요가 있는지?
                        newRoadBike.Typename = jsonRoadBike.body.typename; // 타입 이름이 있으므로...
                        newRoadBike.WheelSize = jsonRoadBike.body.wheel_size;
                        newRoadBike.Modelname = jsonRoadBike.body.modelname;
                        newRoadBike.ColorSize = jsonRoadBike.body.color_size;
                        newRoadBike.FrameNo = jsonRoadBike.body.frame_no;
                        newRoadBike.Characteristics = jsonRoadBike.body.characteristics;
                        newRoadBike.DeviceID = jsonRoadBike.body.device_id;
                        newRoadBike.Pictures = jsonRoadBike.body.pictures;
                        newRoadBike.AvatarLevel = 1;
                        newRoadBike.RidingScore = 0;
                        newRoadBike.RidingCount = 0;
                        newRoadBike.DistanceAccumulated = 0;
                        newRoadBike.TimeAccumulated = 0;
                        newRoadBike.AverageDistance = 0;
                        newRoadBike.AverageRidingTime = 0;
                        newRoadBike.CaloriesAccumulated = 0;
                        newRoadBike.AverageCalories = 0;
                        newRoadBike.Lost = "N";
                        newRoadBike.LastGPS = "";

                        newRoadBike.DT_registered = dtNow;
                        newRoadBike.DT_lastaccess = dtNow;
                        //theBike.USERS_id = thisUser.id; // 오류남.

                        thisUser.ROADBIKEs.Add(newRoadBike);
                        thisUser.DT_lastaccess = dtNow;
                    }
                    else // 기존 바이크 업데이트
                    {

                        //theBike.Nickname = jsonRoadBike.body.nickname;
                        theBike.Type = (byte)jsonRoadBike.body.type; // 타입을 정수로 보관할 필요가 있는지?
                        theBike.Typename = jsonRoadBike.body.typename; // 타입 이름이 있으므로...
                        theBike.WheelSize = jsonRoadBike.body.wheel_size;
                        theBike.Modelname = jsonRoadBike.body.modelname;
                        theBike.ColorSize = jsonRoadBike.body.color_size;
                        theBike.FrameNo = jsonRoadBike.body.frame_no;
                        theBike.Characteristics = jsonRoadBike.body.characteristics;
                        theBike.DeviceID = jsonRoadBike.body.device_id;
                        theBike.Pictures = jsonRoadBike.body.pictures;
                        //theBike.Lost = jsonRoadBike.body.lost; // 분실 도움 요청. --> 신고 및 취소에서 만 반영
                        theBike.DT_lastaccess = dtNow;
                        theBike.LastGPS = jsonRoadBike.body.last_gps;
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
                    , jsonRoadBike==null?"jsonRoadBike is null":jsonRoadBike.header.auth_token, logdb, newLog);
                return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
            }

        }



    }
}
