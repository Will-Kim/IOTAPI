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
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace HKSportsServer.Controllers
{
    public partial class HKSportsController : Controller
    {
        private string contentType= "multipart/form-data; boundary";

        [System.Web.Http.HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> UploadBikeImage(HttpPostedFileBase file)
        {
            DateTime dtNow = DateTime.Now;

            // 객체 초기화
            ResponseSimple rs = new ResponseSimple();
            HKRheader rHeader = new HKRheader();

            LOG newLog = new LOG(); // 로그 객체

            // DB Context 가져오기
            LOGDBDataContext logdb = new LOGDBDataContext(/**/);

            // Log 처리 루틴 -------------------------------------------------
            newLog.action = "UploadBikeImage: ";// + fileName;
            newLog.auth_token = "";
            newLog.json = "";
            newLog.dt_created = dtNow;
            // Log 처리 루틴 -------------------------------------------------

            try
            {
                long len = Request.InputStream.Length;
                AppendLog("Request.InputStream.Length: " + len);

                var streamContent = new StreamContent(Request.InputStream);
                AppendLog("A");
                streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(Request.ContentType);

                var provider = await streamContent.ReadAsMultipartAsync();

                AppendLog("provider.Contents: " + provider.Contents.Count);
                foreach (var httpContent in provider.Contents)
                {
                    var fileName = httpContent.Headers.ContentDisposition.FileName;
                    AppendLog(fileName + ": " + httpContent.Headers.ContentType+","+ httpContent.Headers.ContentLength);
                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        continue;
                    }

                    using (Stream fileContents = await httpContent.ReadAsStreamAsync())
                    {
                        var filePath = "D:\\Images\\" + fileName;
                        filePath = filePath.Replace("\"", "");
                        AppendLog("filePath: " + filePath);
                        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            fileContents.CopyTo(fileStream);
                        }
                    }
                }

                rs.header = rHeader;
                logdb.LOGs.InsertOnSubmit(newLog);

                return Json(rs, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                AppendLog("Error: "+ex.Message);

                // Error: Exception
                rs.header = setHKRheader_Err(rHeader, 301, "[ERROR] Exception: " + ex.Message
                    , "", logdb, newLog);
                return Json(rs, JsonRequestBehavior.AllowGet);
            }

        }



    }
}
