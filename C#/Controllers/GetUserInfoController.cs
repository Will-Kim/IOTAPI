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
        public ActionResult GetUserInfo(int? id)
        {
            Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();
            //AppendLog("[REQ HKSports/LoginController]" + json);

            DateTime today = DateTime.Now;

            // 객체 초기화
            Login login = null;
            UserInfo userInfo = new UserInfo();
            HKRheader rHeader = new HKRheader();
            UserInfoBody uiBody = new UserInfoBody();

            LOG newLog = new LOG(); // 로그 객체

            // DB Context 가져오기
            HKRiderDBDataContext db = new HKRiderDBDataContext(/*connectionString here */);
            LOGDBDataContext logdb = new LOGDBDataContext(/**/);

            try
            {
                login = JsonConvert.DeserializeObject<Login>(json);

                // Log 처리 루틴 -------------------------------------------------
                newLog.action = login.header.action;
                newLog.auth_token = login.header.auth_token;
                newLog.json = json;
                newLog.dt_created = DateTime.Now;
                // Log 처리 루틴 -------------------------------------------------

                if (!login.header.action.Equals("GetUserInfo"))
                {
                    userInfo.header = setHKRheader_Err(rHeader, 101, "[ERROR] Action is wrong: " + login.header.action,
                                            login.header.auth_token, logdb, newLog);
                    return Json(userInfo, JsonRequestBehavior.AllowGet);
                }

                // Version Check 추가 예정

                if (login.header.client_market == null || login.body.email == null)
                {
                    userInfo.header = setHKRheader_Err(rHeader, 102, "[ERROR] No client_market or No user_id",
                                            login.header.auth_token, logdb, newLog);
                    return Json(userInfo, JsonRequestBehavior.AllowGet);
                }

                USER thisUser = null;
                string passwd = login.body.password;
                string token = login.header.auth_token;

                // User 가져오기
                if (passwd != null && passwd.Length > 0)
                {
                    thisUser = (from u in db.USERs
                                where u.EMail == login.body.email &&
                                        u.Client_Market == login.header.client_market &&
                                        u.Passwd == passwd
                                select u).SingleOrDefault();
                }
                else if (token != null && token.Length > 0)
                {
                    thisUser = (from u in db.USERs
                                where u.EMail == login.body.email &&
                                        u.Client_Market == login.header.client_market &&
                                        u.Auth_Token == token
                                select u).SingleOrDefault();
                }

                if (thisUser == null)
                {
                    // Error: 아이디, 패스워드를 다시 확인하세요.
                    userInfo.header = setHKRheader_Err(rHeader, 201, "[ERROR] 아이디, 패스워드를 다시 확인하세요: " +
                                login.body.email, login.header.auth_token, logdb, newLog);
                    return Json(userInfo, JsonRequestBehavior.AllowGet);
                }

                // 토큰 발급: 로그인으로 들어온 경우는 대부분 Expired 된 경우이다.
                if (token != null && token.Length > 0 && !token.Equals(thisUser.Auth_Token)
                            || thisUser.Auth_Token == null || thisUser.Auth_Token == "")
                {
                    token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                    thisUser.Auth_Token = token;
                }

                if (!login.header.auth_device_id.Equals(thisUser.FCM_Token))
                {
                    thisUser.FCM_Token = login.header.auth_device_id;
                }


                db.SubmitChanges();
                logdb.LOGs.InsertOnSubmit(newLog);
                logdb.SubmitChanges();

                userInfo.header = rHeader;
                userInfo.body = uiBody;
                userInfo = getUserInformation(userInfo, thisUser);

                //userInfo.header = rHeader;
                //userInfo.body = uiBody;
                return Json(userInfo, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                // Error: Exception
                userInfo.header = setHKRheader_Err(rHeader, 301, "[ERROR] Exception: " + ex.Message
                    , login == null ? "Login is null" : login.header.auth_token, logdb, newLog);
                return Json(userInfo, JsonRequestBehavior.AllowGet);
            }

        }


    }
}
