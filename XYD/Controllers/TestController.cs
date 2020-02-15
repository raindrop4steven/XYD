﻿using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using Appkiz.Library.Security.Authentication;
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

        public ActionResult TestEvent()
        {
            //var employee = (User.Identity as AppkizIdentity).Employee;

            Stream stream = Request.InputStream;
            stream.Seek(0, SeekOrigin.Begin);
            string json = new StreamReader(stream).ReadToEnd();
            var eventArguments = JsonConvert.DeserializeObject<XYD_Event_Argument>(json, new XYDCellJsonConverter());

            var eventConfig = WorkflowUtil.GetCellEvent(eventArguments.MessageId, eventArguments.CurrentCellValue.Row, eventArguments.CurrentCellValue.Col);
            var eventArray = eventConfig.Event.Split(',').Select(n => n.Trim()).ToList();
            var className = eventArray[0];
            var methodName = eventArray[1];
            var arguments = eventArray.Skip(2).Take(eventArray.Count - 2).Select(n => n.Trim()).ToList();
            var resultArguments = new List<object>();
            foreach (var arg in arguments)
            {
                // 区分一下正常参数和Cell参数吧，不知道会不会用到
                if (arg.StartsWith("#"))
                {
                    var fieldValue = WorkflowUtil.GetFieldValue(eventArguments.Fields, arg);
                    resultArguments.Add(fieldValue);
                }
                else
                {
                    resultArguments.Add(arg);
                }
            }
            var result = caller(className, methodName, resultArguments.Cast<object>().ToArray());
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