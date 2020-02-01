using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Entity;

namespace XYD.Controllers
{
    public class TestController : Controller
    {
        WorkflowMgr mgr = new WorkflowMgr();
        SheetMgr sheetMgr = new SheetMgr();
        OrgMgr orgMgr = new OrgMgr();

        [Authorize]
        public ActionResult TestHeader()
        {
            try
            {
                var serverName = Request.ServerVariables["SERVER_NAME"] + ":" + Request.ServerVariables["SERVER_PORT"];
                return ResponseUtil.OK(serverName);
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
    }
}