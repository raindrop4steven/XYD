using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using Appkiz.Library.Security.Authentication;
using Newtonsoft.Json;
using XYD.Common;
using XYD.Entity;

namespace XYD.Controllers
{
    public class TestController : Controller
    {
        OrgMgr orgMgr = new OrgMgr();
        WorkflowMgr wmgr = new WorkflowMgr();
        
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

        [Authorize]
        public ActionResult pageInfo(string mid, string nid)
        {
            var employee = (User.Identity as AppkizIdentity).Employee;
            XYD_Fields fields = WorkflowUtil.GetWorkflowFields(employee.EmplID, nid, mid);
            return ResponseUtil.OK(fields);
        }
    }
}