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

        #region 审批
        [HttpPost]
        public ActionResult Audit(FormCollection collection)
        {
            var mid = collection["mid"];
            var nid = collection["nid"];
            var operate = collection["operate"];
            var opinion = collection["opinion"];

            WorkflowUtil.AuditMessage(mid, nid, operate, opinion);
            return ResponseUtil.OK("审批OK");
        }
        #endregion

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