using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using Appkiz.Library.Security.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        public ActionResult TestVoucher(string mid, string name)
        {
            XYD_SubVoucherCode SubCode = null;
            Message message = mgr.GetMessage(mid);
            var workflowId = message.FromTemplate;
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("Voucher.json"));

            using (StreamReader sr = new StreamReader(filePathName))
            {
                var config = JsonConvert.DeserializeObject<XYD_VoucherCodes>(sr.ReadToEnd());
                foreach(var VoucherCode in config.VoucherCodes)
                {
                    if (VoucherCode.WorkflowId == workflowId)
                    {
                        if (VoucherCode.Codes.Count == 1)
                        {
                            SubCode = VoucherCode.Codes.FirstOrDefault();
                            break;
                        }
                        else
                        {
                            foreach(var Code in VoucherCode.Codes)
                            {
                                if (Code.Subs.Contains(name))
                                {
                                    SubCode = Code;
                                    break;
                                }
                            }
                        }
                    }
                }
                return ResponseUtil.OK(SubCode);
            }
        }

        public ActionResult TestWeek(DateTime date)
        {
            var firstDay = CommonUtils.FirstDayOfWeek(date);
            var lastDay = CommonUtils.LastDayOfWeek(date);
            return ResponseUtil.OK(new
            {
                firstDay = firstDay,
                lastDay = lastDay
            });
        }

        public async Task<ActionResult> SendEmail(string Tos)
        {
            await Task.Run(() => { new MailHelper().SendAsync("Send Async Email Test", "This is Send Async Email Test", Tos, null); });
            return ResponseUtil.OK("ok");
        }
        /// <summary>
        /// 邮件发送后的回调方法
        /// </summary>
        /// <param name="message"></param>
        static void emailCompleted(string message)
        {
            
        }

        #region 查找单元格
        public ActionResult TestWorksheet(string mid)
        {
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
            Worksheet worksheet = doc.Worksheet;
            int maxRow, maxCol;
            List<Workcell> workCells =  worksheet.FindWorkcells(out maxRow, out maxCol);
            IEnumerable<IGrouping<int, Workcell>> groupCells = workCells.OrderBy(n => n.WorkcellRow).ThenBy(n => n.WorkcellCol).GroupBy(n => n.WorkcellRow);
            List<XYD_Base_Cell> cells = new List<XYD_Base_Cell>();
            var results = new List<object>();
            foreach(var cellGroup in groupCells)
            {
                // 是否是列表标题行
                bool isTitleLine = false;
                // 当前行列数
                int columns = cellGroup.Count();
                // 值单元格列数
                int valueColumns = cellGroup.Where(n => n.WorkcellValue == "").Count();
                // 过滤无效的行
                if (columns < 2) 
                {
                    continue;
                }
                // 判断是单行还是表格
                if (valueColumns == 0)
                {
                    isTitleLine = true;
                }
                if (isTitleLine) // 列表标题行
                {
                    
                }
                else 
                {
                    var cell = new XYD_Single_Cell();
                    // 遍历每行表格
                    foreach(Workcell item in cellGroup) 
                    {
                        if (cell.Value == null) {
                            cell.Value = new XYD_Cell_Value();
                        }
                        if (string.IsNullOrEmpty(item.WorkcellValue)) // 值
                        {
                            cell.Value.Row =int.Parse(item.ID[1].ToString());
                            cell.Value.Col = (int)item.ID[0]-64;
                            cells.Add(cell);
                        }
                        else // 标题
                        {
                            cell = new XYD_Single_Cell(); 
                            cell.Value = new XYD_Cell_Value();
                            cell.Type = 0;
                            cell.Value.Title = item.WorkcellValue;
                        }
                    }
                }
            }
            return ResponseUtil.OK(cells);
        }
        #endregion
    }
}