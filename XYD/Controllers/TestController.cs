using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using Appkiz.Library.Security;
using Appkiz.Library.Security.Authentication;
using Newtonsoft.Json;
using XYD.Common;
using XYD.Entity;

namespace XYD.Controllers
{
    public class TestController : Controller
    {
        [Authorize]
        public ActionResult caculate(FormCollection collection)
        {
            return ResponseUtil.OK(new
            {
                employee = collection["employee"],
                eventArgument = collection["eventArgument"]
            });
        }

        [Authorize]
        public ActionResult redirect()
        {
            var employee = (User.Identity as AppkizIdentity).Employee;

            Stream stream = Request.InputStream;
            stream.Seek(0, SeekOrigin.Begin);
            string json = new StreamReader(stream).ReadToEnd();
            var eventArguments = JsonConvert.DeserializeObject<XYD_Event_Argument>(json, new XYDCellJsonConverter());
            return RedirectToAction("caculate", "Test", new
            {
                employee = employee,
                eventArgument = eventArguments
            });
        }
    }
}