using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public ActionResult TestEvent(string mid, int row, int col)
        {
            var eventConfig = WorkflowUtil.GetCellEvent(mid, row, col);
            var eventArray = eventConfig.Event.Split(',').Select(n => n.Trim()).ToList();
            var className = string.Empty;
            var methodName = string.Empty;
            var result = caller(eventArray[0], eventArray[1], eventArray.Skip(2).Take(eventArray.Count - 2).Cast<object>().ToArray());
            return ResponseUtil.OK(result);
        }

        private static object caller(string myclass, string mymethod, object[] parameters)
        {
            Assembly.GetEntryAssembly();
            return Type.GetType(myclass).GetMethod(mymethod).Invoke((object)null, parameters);
        }

        public ActionResult TestCityLevel(string city)
        {
            var level = WorkflowUtil.GetCityLevel(city);
            return ResponseUtil.OK(level);
        }

        public ActionResult TestUserRole(string emplID)
        {
            var role = WorkflowUtil.GetRoleById(emplID);
            return ResponseUtil.OK(role);
        }
    }
}