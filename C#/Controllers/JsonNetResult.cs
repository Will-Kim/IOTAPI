using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.IO;

namespace HKSportsServer.Controllers
{
    public class JsonNetResult : JsonResult
    {
        public JsonNetResult()
        {
            this.ContentType = "application/json";
        }

        public JsonNetResult(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior jsonRequestBehavior)
        {
            this.ContentEncoding = contentEncoding;
            this.ContentType = !string.IsNullOrWhiteSpace(contentType) ? contentType : "application/json";
            this.Data = data;
            this.JsonRequestBehavior = jsonRequestBehavior;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var response = context.HttpContext.Response;

            response.ContentType = !String.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;

            if (Data == null)
                return;

            // If you need special handling, you can call another form of SerializeObject below
            var serializedObject = JsonConvert.SerializeObject(Data, Formatting.None);
            //AppendLog("[RES]"+(string)serializedObject);
            response.Write(serializedObject);
        }


        private void AppendLog(string log)
        {
            string path = @"C:\logs\hkrider.log";

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
    }
}
