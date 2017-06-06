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
        public ActionResult RemoveRoadBike(int? id)
        {
            Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();
            //AppendLog("[REQ HKSports/LoginController]" + json);

            string actionName = "RemoveRoadBike";
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

                List<RIDINGLOG> removeList = db.RIDINGLOGs.Where(r => r.ROADBIKES_id == jsonRoadBike.body.id).ToList();
                db.RIDINGLOGs.DeleteAllOnSubmit(removeList);

                var removeObj = db.ROADBIKEs.Where(r => r.id == jsonRoadBike.body.id).SingleOrDefault();
                db.ROADBIKEs.DeleteOnSubmit(removeObj);

                // User 가져오기
                USER thisUser = (from u in db.USERs
                                 where u.Auth_Token == jsonRoadBike.header.auth_token
                                 select u).SingleOrDefault();
                thisUser.DT_lastaccess = dtNow;

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
