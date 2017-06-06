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
        // POST: AddRidingLog --> Riding 기록 저장.
        [System.Web.Http.HttpPost]
        public ActionResult AddRidingLog(int? id)
        {
            Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();

            string actionName = "AddRidingLog";
            DateTime dtNow = DateTime.Now;
     
            // 객체 초기화
            JsonRidingLog jsonRidingLog = null;
            UserInfo returnUserInfo = new UserInfo();
            HKRheader rHeader = new HKRheader();
            UserInfoBody uiBody = new UserInfoBody();
            RIDINGLOG ridingLog;

            LOG newLog = new LOG(); // 로그 객체

            // DB Context 가져오기
            HKRiderDBDataContext db = new HKRiderDBDataContext(/*connectionString here */);
            LOGDBDataContext logdb = new LOGDBDataContext(/**/);
            try
            {
                jsonRidingLog = JsonConvert.DeserializeObject<JsonRidingLog>(json);

                // Log 처리 루틴 ------------------------------------------------- 
                newLog.action = jsonRidingLog.header.action;
                newLog.auth_token = jsonRidingLog.header.auth_token;
                newLog.json = json;
                newLog.dt_created = dtNow;
                // Log 처리 루틴 ------------------------------------------------- 
            
                if (!jsonRidingLog.header.action.Equals(actionName))
                {
                    returnUserInfo.header = setHKRheader_Err(rHeader, 101, "[ERROR] Action is wrong: " + jsonRidingLog.header.action,
                                            returnUserInfo.header.auth_token, logdb, newLog);
                    return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
                } else if (jsonRidingLog.header.auth_token == null || jsonRidingLog.header.auth_token == "")
                {
                    returnUserInfo.header = setHKRheader_Err(rHeader, 103, "[ERROR] Auth Token is wrong: " + jsonRidingLog.header.auth_token,
                                            returnUserInfo.header.auth_token, logdb, newLog);
                    return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
                }

                // Version Check 추가 예정

                // User 가져오기
                USER thisUser = (   from    u in db.USERs
                                    where   u.Auth_Token == jsonRidingLog.header.auth_token
                                    select  u).SingleOrDefault();

                if (thisUser == null)
                {
                    //Error 202: 다시 접속하세요. 유저정보를 확인할 수 없습니다.
                    returnUserInfo.header = setHKRheader_Err(rHeader, 202, "[ERROR] 다시 접속하세요. 유저정보를 확인할 수 없습니다: " +
                                jsonRidingLog.header.auth_token, returnUserInfo.header.auth_token, logdb, newLog);
                    return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ROADBIKE theBike = thisUser.ROADBIKEs.Where(r => r.DeviceID == jsonRidingLog.body.mac_address).SingleOrDefault();
                      
                    if (theBike == null) //해당 맥주소가 해당 유저에게는 존재하지 않음.
                    {
                        //Error 203: 주행기록을 저장한 유저정보를 확인할 수 없습니다. 다시 접속하세요.
                        returnUserInfo.header = setHKRheader_Err(rHeader, 203, "[ERROR] 주행기록을 저장한 유저정보를 확인할 수 없습니다. 다시 접속하세요.: " +
                                    thisUser.EMail, thisUser.Auth_Token, logdb, newLog);
                        return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
                    }
                    else // 존재함.
                    {
                        ridingLog = new RIDINGLOG();
                        ridingLog.RidingScore = (int)jsonRidingLog.body.riding_score;
                        ridingLog.Distance = (int)jsonRidingLog.body.distance;
                        ridingLog.Duration = (long)jsonRidingLog.body.duration;
                        ridingLog.DT_Start = FromMillisJava((long)jsonRidingLog.body.dt_start).ToLocalTime();
                        ridingLog.DT_End = FromMillisJava((long)jsonRidingLog.body.dt_end).ToLocalTime();
                        ridingLog.AverageSpeed = (int)jsonRidingLog.body.average_speed;
                        ridingLog.MaxSpeed = (int)jsonRidingLog.body.max_speed;
                        ridingLog.CaloriesBurn = (int)jsonRidingLog.body.calories_burn;
                        ridingLog.StartLocation = jsonRidingLog.body.start_location;
                        ridingLog.EndLocation = jsonRidingLog.body.end_location;
                        ridingLog.GPSrecords = jsonRidingLog.body.gps_records;
                        ridingLog.LastGPS = jsonRidingLog.body.last_gps;
                        if(jsonRidingLog.body.debug_data != null)
                        {
                            ridingLog.DebugData = jsonRidingLog.body.debug_data;
                        }
                        ridingLog.dt_registered = dtNow;
                        ridingLog.ROADBIKES_id = theBike.id;
                        ridingLog.USERS_id = thisUser.id;

                        int ridingCnt = theBike.RidingCount+1;
                        int totalCals = theBike.CaloriesAccumulated + (int)jsonRidingLog.body.calories_burn;
                        int totalDistance = theBike.DistanceAccumulated + (int)jsonRidingLog.body.distance;
                        int totalRidingT = theBike.TimeAccumulated + (int)jsonRidingLog.body.duration;
                        
                        int score4max = 0;
                        if (theBike.MaxSpeed < (int)jsonRidingLog.body.max_speed)
                        {
                            theBike.MaxSpeed = (int)jsonRidingLog.body.max_speed;
                            score4max = (int)(((int)jsonRidingLog.body.max_speed / 3.6) - (theBike.MaxSpeed / 3.6));
                        }
                        int ridingScore = theBike.RidingScore + (int)jsonRidingLog.body.riding_score + score4max;

                        theBike.RidingScore = ridingScore;
                        theBike.AvatarLevel = GetLevel(ridingScore);
                        theBike.RidingCount = ridingCnt;
                        theBike.CaloriesAccumulated = totalCals;
                        theBike.AverageCalories = totalCals / ridingCnt;
                        theBike.DistanceAccumulated = totalDistance;
                        theBike.AverageDistance = totalDistance / ridingCnt;
                        theBike.AverageRidingTime = totalRidingT / ridingCnt;
                        theBike.AverageSpeed = totalDistance / totalRidingT; //m per sec
                        theBike.TimeAccumulated = totalRidingT;
                        theBike.LastGPS = jsonRidingLog.body.last_gps;
                        theBike.RIDINGLOGs.Add(ridingLog);
                        theBike.DT_lastaccess = dtNow;
                        thisUser.DT_lastaccess = dtNow;
                    }
                }

                db.SubmitChanges();
                logdb.LOGs.InsertOnSubmit(newLog);

                rHeader.ret_string = ridingLog.id.ToString();
                returnUserInfo.header = rHeader;
                returnUserInfo.body = uiBody;
                returnUserInfo = getUserInformation(returnUserInfo, thisUser); // 다시 계정 정보를 받아서 리턴해 준다.

                return Json(returnUserInfo, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                // Error: Exception
                returnUserInfo.header = setHKRheader_Err(rHeader, 301, "[ERROR] Exception: " + ex.Message
                    , jsonRidingLog==null?"jsonRidingLog is null":jsonRidingLog.header.auth_token, logdb, newLog);
                return Json(returnUserInfo, JsonRequestBehavior.AllowGet);
            }

        }

        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public DateTime FromMillisJava(long milliseconds)
        {
            return UnixEpoch.AddMilliseconds(milliseconds);
        }

        public int GetLevel(int score)
        {
            int level = 1;

            if (score >= 10 && score < 20) { level = 2; }
            else if (score >= 20 && score < 40) { level = 3; }
            else if (score >= 40 && score < 80) { level = 4; }
            else if (score >= 80 && score < 136) { level = 5; }
            else if (score >= 136 && score < 231) { level = 6; }
            else if (score >= 231 && score < 393) { level = 7; }
            else if (score >= 393 && score < 668) { level = 8; }
            else if (score >= 668 && score < 1136) { level = 9; }
            else if (score >= 1136 && score < 1590) { level = 10; }
            else if (score >= 1590 && score < 2226) { level = 11; }
            else if (score >= 2226 && score < 3117) { level = 12; }
            else if (score >= 3117 && score < 4364) { level = 13; }
            else if (score >= 4364 && score < 6109) { level = 14; }
            else if (score >= 6109 && score < 7331) { level = 15; }
            else if (score >= 7331 && score < 8797) { level = 16; }
            else if (score >= 8797 && score < 10556) { level = 17; }
            else if (score >= 10556 && score < 12668) { level = 18; }
            else if (score >= 12668 && score < 15201) { level = 19; }
            else if (score >= 15201 && score < 18242) { level = 20; }
            else if (score >= 18242 && score < 21890) { level = 21; }
            else if (score >= 21890 && score < 26268) { level = 22; }
            else if (score >= 26268 && score < 31521) { level = 23; }
            else if (score >= 31521 && score < 37826) { level = 24; }
            else if (score >= 37826 && score < 45391) { level = 25; }
            else if (score >= 45391 && score < 54469) { level = 26; }
            else if (score >= 54469 && score < 65363) { level = 27; }
            else if (score >= 65363 && score < 78435) { level = 28; }
            else if (score >= 78435 && score < 94123) { level = 29; }
            else if (score >= 94123 && score < 112947) { level = 30; }
            else if (score >= 112947 && score < 135536) { level = 31; }
            else if (score >= 135536 && score < 162644) { level = 32; }
            else if (score >= 162644 && score < 195172) { level = 33; }
            else if (score >= 195172 && score < 234207) { level = 34; }
            else if (score >= 234207 && score < 281048) { level = 35; }
            else if (score >= 281048 && score < 337258) { level = 36; }
            else if (score >= 337258 && score < 404710) { level = 37; }
            else if (score >= 404710 && score < 485651) { level = 38; }
            else if (score >= 485651 && score < 582782) { level = 39; }
            else if (score >= 582782 && score < 699338) { level = 40; }
            else if (score >= 699338 && score < 839206) { level = 41; }
            else if (score >= 839206 && score < 1007047) { level = 42; }
            else if (score >= 1007047 && score < 1208456) { level = 43; }
            else if (score >= 1208456 && score < 1450147) { level = 44; }
            else if (score >= 1450147 && score < 1740177) { level = 45; }
            else if (score >= 1740177 && score < 2088212) { level = 46; }
            else if (score >= 2088212 && score < 2505855) { level = 47; }
            else if (score >= 2505855 && score < 3007026) { level = 48; }
            else if (score >= 3007026 && score < 3608431) { level = 49; }
            else if (score >= 3608431 && score < 3969274) { level = 50; }
            else if (score >= 3969274 && score < 4366201) { level = 51; }
            else if (score >= 4366201 && score < 4802821) { level = 52; }
            else if (score >= 4802821 && score < 5283104) { level = 53; }
            else if (score >= 5283104 && score < 5811414) { level = 54; }
            else if (score >= 5811414 && score < 6392555) { level = 55; }
            else if (score >= 6392555 && score < 7031811) { level = 56; }
            else if (score >= 7031811 && score < 7734992) { level = 57; }
            else if (score >= 7734992 && score < 8508491) { level = 58; }
            else if (score >= 8508491 && score < 9359340) { level = 59; }
            else if (score >= 9359340 && score < 10295274) { level = 60; }
            else if (score >= 10295274 && score < 11324802) { level = 61; }
            else if (score >= 11324802 && score < 12457282) { level = 62; }
            else if (score >= 12457282 && score < 13703010) { level = 63; }
            else if (score >= 13703010 && score < 15073311) { level = 64; }
            else if (score >= 15073311 && score < 16580642) { level = 65; }
            else if (score >= 16580642 && score < 18238706) { level = 66; }
            else if (score >= 18238706 && score < 20062577) { level = 67; }
            else if (score >= 20062577 && score < 22068835) { level = 68; }
            else if (score >= 22068835 && score < 24275718) { level = 69; }
            else if (score >= 24275718 && score < 26703290) { level = 70; }
            else if (score >= 26703290 && score < 29373619) { level = 71; }
            else if (score >= 29373619 && score < 32310981) { level = 72; }
            else if (score >= 32310981 && score < 35542079) { level = 73; }
            else if (score >= 35542079 && score < 39096287) { level = 74; }
            else if (score >= 39096287 && score < 43005916) { level = 75; }
            else if (score >= 43005916 && score < 47306507) { level = 76; }
            else if (score >= 47306507 && score < 52037158) { level = 77; }
            else if (score >= 52037158 && score < 57240874) { level = 78; }
            else if (score >= 57240874 && score < 62964961) { level = 79; }
            else if (score >= 62964961 && score < 69261457) { level = 80; }
            else if (score >= 69261457 && score < 76187603) { level = 81; }
            else if (score >= 76187603 && score < 83806363) { level = 82; }
            else if (score >= 83806363 && score < 92186999) { level = 83; }
            else if (score >= 92186999 && score < 101405699) { level = 84; }
            else if (score >= 101405699 && score < 111546269) { level = 85; }
            else if (score >= 111546269 && score < 122700896) { level = 86; }
            else if (score >= 122700896 && score < 134970986) { level = 87; }
            else if (score >= 134970986 && score < 148468084) { level = 88; }
            else if (score >= 148468084 && score < 163314893) { level = 89; }
            else if (score >= 163314893 && score < 179646382) { level = 90; }
            else if (score >= 179646382 && score < 197611020) { level = 91; }
            else if (score >= 197611020 && score < 217372122) { level = 92; }
            else if (score >= 217372122 && score < 239109335) { level = 93; }
            else if (score >= 239109335 && score < 263020268) { level = 94; }
            else if (score >= 263020268 && score < 289322295) { level = 95; }
            else if (score >= 289322295 && score < 318254524) { level = 96; }
            else if (score >= 318254524 && score < 350079977) { level = 97; }
            else if (score >= 350079977 && score < 385087974) { level = 98; }
            else if (score >= 385087974) { level = 99; }

            return level;
        }

    }
}