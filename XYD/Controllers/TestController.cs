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
using static XYD.Common.DEP_Constants;

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

        #region 解析工作流表单
        public ActionResult TestWorksheet(string mid)
        {
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
            Worksheet worksheet = doc.Worksheet;
            int maxRow, maxCol;
            List<Workcell> workCells =  worksheet.FindWorkcells(out maxRow, out maxCol);
            var groupCells = workCells.OrderBy(n => n.WorkcellRow).ThenBy(n => n.WorkcellCol).GroupBy(n => n.WorkcellRow);
            // 判断每行类型
            var worksheetDict = IdentifySheet(groupCells);
            // 生成表单
            var fields = GenerateFields(worksheetDict);
            return ResponseUtil.OK(fields);
        }
        #endregion

        #region 鉴别表单每行类型
        public List<KeyValuePair<WORKSHEET_LINE_TYPE, IEnumerable<Workcell>>> IdentifySheet(IEnumerable<IGrouping<int, Workcell>> groupCells)
        {
            var worksheetDict = new List<KeyValuePair<WORKSHEET_LINE_TYPE, IEnumerable<Workcell>>>();
            var haveTableHeader = false;
            foreach (var cellGroup in groupCells)
            {
                WORKSHEET_LINE_TYPE type;
                int lineColumns = cellGroup.Count();
                int valueColumns = cellGroup.Where(n => n.WorkcellValue == "").Count();
                // 过滤无效的行
                if (lineColumns < 2)
                {
                    continue;
                }
                // 判断是单行还是表格
                if (valueColumns == 0)
                {
                    if (cellGroup.Any(n => n.WorkcellValue == "审批节点"))
                    {
                        // 审核节点，去掉
                        break;
                    }
                    else
                    {
                        type = WORKSHEET_LINE_TYPE.TABLE_HEADER;
                        haveTableHeader = true;
                    }
                }
                else if (valueColumns == lineColumns)
                {
                    if (haveTableHeader)
                    {
                        type = WORKSHEET_LINE_TYPE.TABLE_DATA;
                    }
                    else
                    {
                        // 无效的空行
                        continue;
                    }
                }
                else
                {
                    type = WORKSHEET_LINE_TYPE.NORMAL;
                }
                worksheetDict.Add(new KeyValuePair<WORKSHEET_LINE_TYPE, IEnumerable<Workcell>>(type, cellGroup));
            }
            return worksheetDict;
        }
        #endregion

        #region 生成表单
        public XYD_Fields GenerateFields(List<KeyValuePair<WORKSHEET_LINE_TYPE, IEnumerable<Workcell>>> worksheetDict)
        {
            XYD_Fields fields = new XYD_Fields();
            fields.Fields = new List<XYD_Base_Cell>();
            // 数据标题
            var tableHeaders = new List<string>();
            // 数据行
            var arrayCell = new XYD_Array_Cell();
            arrayCell.Type = 3;
            arrayCell.Array = new List<List<XYD_Cell_Value>>();
            foreach (var item in worksheetDict)
            {
                // 单行
                if (WORKSHEET_LINE_TYPE.NORMAL == item.Key)
                {
                    // 如果table有值，则加进去，并清空
                    if (tableHeaders.Count > 0)
                    {
                        fields.Fields.Add(arrayCell);
                        tableHeaders.Clear(); // FIXME:目前这一只支持一个TABLE，如果清空后不影响之前的，可以支持多个
                    }
                    // 标题
                    var title = string.Empty;
                    // 拆分出标题和值
                    foreach(Workcell cell in item.Value)
                    {
                        var workCellValue = cell.WorkcellValue;
                        if (string.IsNullOrEmpty(workCellValue)) // 标题
                        {
                            var cellField = new XYD_Single_Cell();
                            cellField.Value = new XYD_Cell_Value();
                            cellField.Type = 0;
                            cellField.Value.Row = cell.WorkcellRow;
                            cellField.Value.Col = cell.WorkcellCol;
                            cellField.Value.Type = 0; // TODO: 需判断
                            cellField.Value.Required = true; // TODO
                            cellField.Value.Title = title;
                            cellField.Value.NeedRefresh = false; // TODO
                            cellField.Value.Options = null; // TODO
                            fields.Fields.Add(cellField);
                        }
                        else
                        {
                            title = workCellValue;
                        }
                    }
                }
                else if (WORKSHEET_LINE_TYPE.TABLE_HEADER == item.Key)
                {
                    foreach(Workcell cell in item.Value)
                    {
                        tableHeaders.Add(cell.WorkcellValue);
                    }
                }
                else if (WORKSHEET_LINE_TYPE.TABLE_DATA == item.Key)
                {
                    var tableDatas = new List<XYD_Cell_Value>();
                    for(int i = 0; i < item.Value.Count(); i++)
                    {
                        Workcell cell = item.Value.ElementAt(i);
                        var headerTitle = tableHeaders.ElementAt(i);
                        var cellValue = new XYD_Cell_Value();
                        cellValue.Type = 0; // TODO
                        cellValue.Row = cell.WorkcellRow;
                        cellValue.Col = cell.WorkcellCol;
                        cellValue.Type = 0; // TODO: 需判断
                        cellValue.Required = true; // TODO
                        cellValue.Title = headerTitle;
                        cellValue.NeedRefresh = false; // TODO
                        cellValue.Options = null; // TODO
                        tableDatas.Add(cellValue);
                    }
                    arrayCell.Array.Add(tableDatas);
                    // 如果是整个表单最后一个元素，则直接加到生成的列表中去
                    if (worksheetDict.IndexOf(item) == worksheetDict.Count()-1)
                    {
                        fields.Fields.Add(arrayCell);
                        tableHeaders.Clear();
                    }
                }
                else
                {
                    throw new Exception("不支持的行类型");
                }
            }
            return fields;
        }
        #endregion
    }
}