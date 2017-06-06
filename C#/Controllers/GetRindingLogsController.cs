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
        public ActionResult GetRidingLogs(int? id)
        {
            Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();

            string actionName = "GetRidingLogs";
            DateTime dtNow = DateTime.Now;

            // 객체 초기화
            JsonGetRidingLogs jsonGetRidingLogs = null;
            ResponseRidingLogs resRidingLogs = new ResponseRidingLogs();
            HKRheader rHeader = new HKRheader();
            List<RidingLog> Body = new List<RidingLog>();

            LOG newLog = new LOG(); // 로그 객체

            // DB Context 가져오기
            HKRiderDBDataContext db = new HKRiderDBDataContext(/*connectionString here */);
            LOGDBDataContext logdb = new LOGDBDataContext(/**/);
            try
            {
                jsonGetRidingLogs = JsonConvert.DeserializeObject<JsonGetRidingLogs>(json);

                // Log 처리 루틴 -------------------------------------------------
                newLog.action = jsonGetRidingLogs.header.action;
                newLog.auth_token = jsonGetRidingLogs.header.auth_token;
                newLog.json = json;
                newLog.dt_created = dtNow;
                // Log 처리 루틴 -------------------------------------------------

                if (!jsonGetRidingLogs.header.action.Equals(actionName))
                {
                    resRidingLogs.header = setHKRheader_Err(rHeader, 101, "[ERROR] Action is wrong: " + jsonGetRidingLogs.header.action,
                                            resRidingLogs.header.auth_token, logdb, newLog);
                    return Json(resRidingLogs, JsonRequestBehavior.AllowGet);
                } else if (jsonGetRidingLogs.header.auth_token == null || jsonGetRidingLogs.header.auth_token == "")
                {
                    resRidingLogs.header = setHKRheader_Err(rHeader, 103, "[ERROR] Auth Token is wrong: " + jsonGetRidingLogs.header.auth_token,
                                            resRidingLogs.header.auth_token, logdb, newLog);
                    return Json(resRidingLogs, JsonRequestBehavior.AllowGet);
                }

                // Version Check 추가 예정

                // User 가져오기
                USER thisUser = (   from    u in db.USERs
                                    where   u.Auth_Token == jsonGetRidingLogs.header.auth_token
                                    select  u).SingleOrDefault();

                if (thisUser == null)
                {
                    //Error 202: 다시 접속하세요. 유저정보를 확인할 수 없습니다.
                    resRidingLogs.header = setHKRheader_Err(rHeader, 202, "[ERROR] 다시 접속하세요. 유저정보를 확인할 수 없습니다: ",
                                                            resRidingLogs.header.auth_token, logdb, newLog);
                    return Json(resRidingLogs, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ROADBIKE theBike = thisUser.ROADBIKEs.Where(r => r.DeviceID == jsonGetRidingLogs.body.mac_address).SingleOrDefault();

                    if (theBike == null) //해당 맥주소가 해당 유저에게는 존재하지 않음.
                    {
                        //Error 203: 주행기록을 저장한 유저정보를 확인할 수 없습니다. 다시 접속하세요.
                        resRidingLogs.header = setHKRheader_Err(rHeader, 203, "[ERROR] 주행기록을 저장한 유저정보를 확인할 수 없습니다. 다시 접속하세요.: ",
                                                                resRidingLogs.header.auth_token, logdb, newLog);
                        return Json(resRidingLogs, JsonRequestBehavior.AllowGet);
                    }
                    else // 존재함.
                    {
                        if (jsonGetRidingLogs.body.remove_id > 0)
                        {
                            var rDel = db.RIDINGLOGs.Where(r => r.id == jsonGetRidingLogs.body.remove_id).SingleOrDefault();
                            db.RIDINGLOGs.DeleteOnSubmit(rDel);
                            db.SubmitChanges();
                        }

                        DateTime FromDt = FromMillisJava((long)jsonGetRidingLogs.body.from_date).ToLocalTime();
                        DateTime ToDt = FromMillisJava((long)jsonGetRidingLogs.body.to_date).ToLocalTime();
                        ToDt = ToDt.AddDays(1); // 하루 추가.

                        var ridingLogs = (from l in db.RIDINGLOGs
                                          where l.USERS_id == thisUser.id &&
                                                l.ROADBIKES_id == theBike.id &&
                                                l.DT_Start >= FromDt &&
                                                l.DT_Start < ToDt
                                          select l);

                        List<RidingLog> listRidingLogs = new List<RidingLog>();
                        foreach(var rl in ridingLogs)
                        {
                            RidingLog newRL = new RidingLog();
                            newRL.id = rl.id;
                            newRL.average_speed = rl.AverageSpeed;
                            newRL.calories_burn = rl.CaloriesBurn;
                            newRL.distance = rl.Distance;
                            newRL.dt_end = DateTime2Long(rl.DT_End);
                            newRL.dt_start = DateTime2Long(rl.DT_Start);
                            newRL.duration = rl.Duration;
                            newRL.start_location = rl.StartLocation;
                            newRL.end_location = rl.EndLocation;
                            newRL.mac_address = theBike.DeviceID;
                            newRL.max_speed = rl.MaxSpeed;
                            newRL.riding_score = rl.RidingScore;
                            newRL.gps_records = rl.GPSrecords;
                            newRL.last_gps = rl.LastGPS;
                            listRidingLogs.Add(newRL);
                        }
                        Body = listRidingLogs;
                    }
                }

                db.SubmitChanges();
                logdb.LOGs.InsertOnSubmit(newLog);

                resRidingLogs.header = rHeader;
                resRidingLogs.body = Body;

                return Json(resRidingLogs, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                // Error: Exception
                resRidingLogs.header = setHKRheader_Err(rHeader, 301, "[ERROR] Exception: " + ex.Message
                    , jsonGetRidingLogs == null?"jsonGetRidingLogs is null": jsonGetRidingLogs.header.auth_token, logdb, newLog);
                return Json(resRidingLogs, JsonRequestBehavior.AllowGet);
            }

        }

        public long DateTime2Long(DateTime dt)
        {
            //dt.AddHours(3);
            // 3시간 적게되는 이유? 시작시간 기준이 새벽0시인데 +9(서울)을 더하니, -3?
            // 윈도우즈는 새벽 0시 기준이고, 자바는 정오(12시) 기준인가?
            TimeSpan javaDiff = dt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)javaDiff.TotalMilliseconds;
        }

    }
}
