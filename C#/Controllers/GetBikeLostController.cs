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
        // POST: RemoveBikeLost
        [System.Web.Http.HttpPost]
        public ActionResult GetBikeLost(int? id)
        {
            Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();
            //AppendLog("[REQ HKSports/LoginController]" + json);

            string actionName = "GetBikeLost";
            DateTime dtNow = DateTime.Now;
     
            // 객체 초기화
            JsonBikeLost jsonBikeLost = null;
            JsonBikeLost returnBikeLost = new JsonBikeLost();
            HKRheader rHeader = new HKRheader();
            BikeLost bikeLostBody = new BikeLost();

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
                    returnBikeLost.header = setHKRheader_Err(rHeader, 101, "[ERROR] Action is wrong: " + jsonBikeLost.header.action,
                                            jsonBikeLost.header.auth_token, logdb, newLog);
                    return Json(returnBikeLost, JsonRequestBehavior.AllowGet);
                } else if (jsonBikeLost.header.auth_token == null || jsonBikeLost.header.auth_token == "")
                {
                    returnBikeLost.header = setHKRheader_Err(rHeader, 103, "[ERROR] Auth Token is wrong: " + jsonBikeLost.header.auth_token,
                                            jsonBikeLost.header.auth_token, logdb, newLog);
                    return Json(returnBikeLost, JsonRequestBehavior.AllowGet);
                }

                // Version Check 추가 예정

                // User 가져오기
                USER thisUser = (   from    u in db.USERs
                                    where   u.Auth_Token == jsonBikeLost.header.auth_token
                                    select  u).SingleOrDefault();

                if (thisUser == null) // 유저가 존재하는지 확인
                {
                    //Error 202: 다시 접속하세요. 유저정보를 확인할 수 없습니다.
                    returnBikeLost.header = setHKRheader_Err(rHeader, 202, "[ERROR] 다시 접속하세요. 유저정보를 확인할 수 없습니다: " +
                                thisUser.EMail, thisUser.Auth_Token, logdb, newLog);
                    return Json(returnBikeLost, JsonRequestBehavior.AllowGet);
                }
                else // 분실신고 바이크의 정보가 있는지 확인.
                {
                    ROADBIKE theBike = db.ROADBIKEs.Where(r => r.Nickname == jsonBikeLost.body.nickname).SingleOrDefault();
                      
                    if (theBike == null) //존재하지 않는 바이크.
                    {
                        //Error 205: 아바타 정보를 확인할 수 없습니다. 계속 문제 발생시 다시 로그인해 주세요.
                        returnBikeLost.header = setHKRheader_Err(rHeader, 205, "[ERROR] 아바타 정보를 확인할 수 없습니다. 계속 문제 발생시 다시 로그인해 주세요.: " +
                                    thisUser.EMail, thisUser.Auth_Token, logdb, newLog);
                        return Json(returnBikeLost, JsonRequestBehavior.AllowGet);
                    }
                    else // 분실 신고 테이블에서 조회.
                    {
                        LOSTBIKE lostBike = (from l in db.LOSTBIKEs
                                             where l.BikeNickname == jsonBikeLost.body.nickname
                                                   && l.Status.Contains("L")
                                             select l).SingleOrDefault();
                        if (lostBike == null) //존재하지 않는 분실신고.
                        {
                            //Error 208: 조회할 분실신고가 없습니다.
                            returnBikeLost.header = setHKRheader_Err(rHeader, 208, "[ERROR] 조회할 분실신고가 없습니다.: " +
                                        thisUser.EMail, thisUser.Auth_Token, logdb, newLog);
                            return Json(returnBikeLost, JsonRequestBehavior.AllowGet);
                        } else
                        {
                            bikeLostBody.characteristics = lostBike.Characteristics;
                            bikeLostBody.color_size = lostBike.ColorSize;
                            bikeLostBody.device_id = lostBike.DeviceID;
                            bikeLostBody.extra_info = lostBike.ExtraInfo;
                            bikeLostBody.frame_no = lostBike.FrameNo;
                            bikeLostBody.location_lost = lostBike.LocationLost;
                            bikeLostBody.modelname = lostBike.ModelName;
                            bikeLostBody.nickname = lostBike.BikeNickname;
                            bikeLostBody.phone_no = lostBike.PhoneNo;
                            bikeLostBody.status = lostBike.Status;
                            bikeLostBody.username = lostBike.UserName;
                            bikeLostBody.dt_lost = lostBike.DT_lost;
                        }
                    }
                }

                logdb.LOGs.InsertOnSubmit(newLog);


                returnBikeLost.header = rHeader;
                returnBikeLost.body = bikeLostBody;

                return Json(returnBikeLost, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                // Error: Exception
                returnBikeLost.header = setHKRheader_Err(rHeader, 301, "[ERROR] Exception: " + ex.Message
                    , jsonBikeLost==null?"jsonRoadBike is null": jsonBikeLost.header.auth_token, logdb, newLog);
                return Json(returnBikeLost, JsonRequestBehavior.AllowGet);
            }

        }



    }
}