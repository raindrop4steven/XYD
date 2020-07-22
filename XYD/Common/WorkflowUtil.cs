using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Common;
using Appkiz.Library.Notification;
using Appkiz.Library.Security;
using XYD.Entity;
using Jiguang.JPush;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using XYD.Models;
using System.Text.RegularExpressions;

namespace XYD.Common
{
    public class WorkflowUtil
    {
        /*
         * 变量定义
         */
        // 通知管理
        private static NotificationMgr notifyMgr = new NotificationMgr();
        // 用户管理
        private static OrgMgr orgMgr = new OrgMgr();
        // 工作流管理
        private static WorkflowMgr mgr = new WorkflowMgr();
        // 表单管理
        private static SheetMgr sheetMgr = new SheetMgr();

        #region 根据流程ID获取对应版本配置路径
        /// <summary>
        /// 根据模版ID获取配置路径
        /// </summary>
        /// <param name="templateID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetConfigPath(string templateID, string mid, string type)
        {
            string version = GetConfigVersion(templateID, mid);
            string folderPath = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], templateID, version);
            string configName = string.Empty;
            if (type == DEP_Constants.Config_Type_Main)
            {
                configName = "main.json";
            }
            else if (type == DEP_Constants.Config_Type_App)
            {
                configName = "app-transformer.json";
            }
            else if (type == DEP_Constants.Config_Type_Audit)
            {
                configName = "audit.json";
            }
            else if (type == DEP_Constants.Config_Type_Start)
            {
                configName = "start.json";
            }
            else if (type == DEP_Constants.Config_Type_Event)
            {
                configName = "event.json";
            }
            else
            {
                throw new Exception("配置类型不存在");
            }
            return Path.Combine(folderPath, configName);
        }
        #endregion

        #region 获得配置
        public static DEP_Mapping GetNodeMappings(string mid, string nid)
        {
            /*
             * 变量定义
             */
            // 节点配置
            DEP_Mapping mappings = null;

            /*
             * 根据模板ID获得对应的配置
             */
            Message message = mgr.GetMessage(mid);
            var templateID = message.FromTemplate;
            var defaultVersion = GetDefaultConfigVersion(templateID);
            var filePathName = GetConfigPath(templateID, mid, DEP_Constants.Config_Type_Main);
            using (StreamReader sr = new StreamReader(filePathName))
            {
                var config = JsonConvert.DeserializeObject<DEP_Node>(sr.ReadToEnd());
                // 根据nid获取对应配置
                foreach (var nodeAction in config.values.mappings)
                {
                    if (nodeAction.key == nid)
                    {
                        mappings = nodeAction;
                    }
                    else
                    {
                        continue;
                    }
                }
                return mappings;
            }
        }
        #endregion

        #region 获得表名
        public static string GetTableName(string mid)
        {
            return "XYD_ReceiveFile";
        }
        #endregion

        #region 获得详情配置
        public static List<DEP_Detail> GetNodeDetail(string mid)
        {
            try
            {
                /*
             * 根据模板ID获得对应的配置
             */
                Message message = mgr.GetMessage(mid);
                var templateID = message.FromTemplate;

                var filePathName = GetConfigPath(templateID, message.MessageID, DEP_Constants.Config_Type_Main);
                //var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", templateID));

                using (StreamReader sr = new StreamReader(filePathName))
                {
                    var config = JsonConvert.DeserializeObject<DEP_Node>(sr.ReadToEnd());

                    return config.values.details;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 获得节点动作
        public static Dictionary<string, object> GetNodeAction(string mid, string nid)
        {
            try
            {
                /*
                 * 根据模板ID获得对应的配置
                 */
                Dictionary<string, object> dict = new Dictionary<string, object>();

                DEP_Action resultAction = null;

                Message message = mgr.GetMessage(mid);
                var templateID = message.FromTemplate;

                var filePathName = GetConfigPath(templateID, message.MessageID, DEP_Constants.Config_Type_Main);
                //var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", templateID));

                using (StreamReader sr = new StreamReader(filePathName))
                {
                    var config = JsonConvert.DeserializeObject<DEP_Node>(sr.ReadToEnd());

                    foreach (var action in config.values.actions)
                    {
                        if (action.key == nid)
                        {
                            resultAction = action;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    // 判断动作配置是否为空
                    if (resultAction == null)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var actionField in resultAction.value)
                        {
                            dict.Add(actionField.key, actionField.value);
                        }

                        return dict;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 获得节点权限
        public static object GetNodeControl(string mid, string nid)
        {
            try
            {
                /*
                 * 根据模板ID获得对应的配置
                 */
                Dictionary<string, object> dict = new Dictionary<string, object>();

                DEP_Control resultControl = null;

                Message message = mgr.GetMessage(mid);
                var templateID = message.FromTemplate;

                var filePathName = GetConfigPath(templateID, message.MessageID, DEP_Constants.Config_Type_Main);
                //var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", templateID));

                using (StreamReader sr = new StreamReader(filePathName))
                {
                    var config = JsonConvert.DeserializeObject<DEP_Node>(sr.ReadToEnd());

                    foreach (var control in config.values.controls)
                    {
                        if (control.key == nid)
                        {
                            resultControl = control;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    return resultControl == null ? null : resultControl.value;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 获得流程节点对应的输入控件类型
        public static object GetNodeInputTypes(string mid, string nid)
        {
            try
            {
                /*
                 * 根据模板ID获得对应的配置
                 */
                Dictionary<string, object> dict = new Dictionary<string, object>();

                DEP_InputType resultInputType = null;

                Message message = mgr.GetMessage(mid);
                var templateID = message.FromTemplate;

                var filePathName = GetConfigPath(templateID, message.MessageID, DEP_Constants.Config_Type_Main);
                //var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", templateID));

                using (StreamReader sr = new StreamReader(filePathName))
                {
                    var config = JsonConvert.DeserializeObject<DEP_Node>(sr.ReadToEnd());

                    foreach (var inputType in config.values.inputTypes)
                    {
                        if (inputType.key == nid)
                        {
                            resultInputType = inputType;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    return resultInputType == null ? null : resultInputType.value;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion

        #region 获得流程转换配置
        public static string GetAppTransformer(string mid)
        {
            try
            {
                /*
                 * 根据模板ID获得对应的配置
                 */
                Dictionary<string, object> dict = new Dictionary<string, object>();

                Message message = mgr.GetMessage(mid);
                var templateID = message.FromTemplate;

                var filePathName = GetConfigPath(templateID, message.MessageID, DEP_Constants.Config_Type_App);
                //var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}-app-transformer.json", templateID));

                using (StreamReader sr = new StreamReader(filePathName))
                {
                    var transformer = sr.ReadToEnd();

                    return transformer;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 填充cell的值
        public static XYD_Base_Cell FillCellValue(string emplId, string NodeId, string MessageID, Worksheet worksheet, XYD_Base_Cell cell)
        {
            Dictionary<string, int> nodeFeildDict = GetNodeFieldDict(MessageID, NodeId);
            if (cell.Type == 0)
            {
                var singleCell = (XYD_Single_Cell) cell;
                singleCell.Value = ParseInnerCell(emplId, NodeId, MessageID, worksheet, singleCell.Value, nodeFeildDict);
                return singleCell;
            }
            else if (cell.Type == 3)
            {
                var arrayCell = (XYD_Array_Cell)cell;
                foreach (List<XYD_Cell_Value> rowCells in arrayCell.Array)
                {
                    for (int i= 0; i < rowCells.Count; i++)
                    {
                        var innerCell = rowCells[i];
                        rowCells[i] = ParseInnerCell(emplId, NodeId, MessageID, worksheet, innerCell, nodeFeildDict);
                    }
                }

                return arrayCell;
            }
            else
            {
                throw new Exception("不支持的类型");
            }
        }
        #endregion

        #region 解析内部Cell
        public static XYD_Cell_Value ParseInnerCell(string emplId, string NodeId, string MessageID, Worksheet worksheet, 
            XYD_Cell_Value innerCell, Dictionary<string, int> NodeFieldDict)
        {
            var workcell = worksheet.GetWorkcell(innerCell.Row, innerCell.Col);
            if (innerCell.Type != 10)
            {
                innerCell.Value = workcell.WorkcellValue;
                innerCell.InterValue = workcell.WorkcellInternalValue;
            }
            else
            {
                // 10，附件
                var attachments = new List<object>();
                var internalAttachs = workcell.WorkcellInternalValue.Split(';').ToList();
                foreach (var attachId in internalAttachs)
                {
                    if (string.IsNullOrEmpty(attachId))
                    {
                        continue;
                    }
                    else
                    {
                        var attachment = mgr.GetAttachment(attachId);
                        attachments.Add(attachment);
                    }
                }
                innerCell.Atts = attachments;
            }
            // 解析CanEdit和Required在前，最后解析#customFunc
            if (innerCell.CanEdit != null && innerCell.CanEdit is bool)
            {
                var canEdit = false;
                var required = false;
                var workcellId = ConvertToWorkcellId(innerCell.Row, innerCell.Col);
                // Cell是否能编辑包含2个条件：
                // 1. Cell是否在改节点NodeField中
                // 2. Cell的控件类型是否是可编辑的
                if (NodeFieldDict.ContainsKey(workcellId))
                {
                    int control = NodeFieldDict[workcellId];
                    canEdit = !isReadonlyCell(workcell.WorkcellDataSource);
                    required = innerCell.Required ? true : control == 2; // 1:可空；2：必填
                }
                innerCell.CanEdit = canEdit;
                innerCell.Required = required;
            }
            
            innerCell = ReflectionUtil.ParseCellValue(emplId, NodeId, MessageID, innerCell);
            return innerCell;
        }
        #endregion

        #region 判断Cell是否可以编辑
        public static bool isReadonlyCell(Enum_WorkcellDataSource source)
        {   
            // //以下类型不允许单元格有“进入”的动作
            var readonlySource = new List<Enum_WorkcellDataSource>()
            {
                Enum_WorkcellDataSource.AutoNum,
                Enum_WorkcellDataSource.DataGrid,
                Enum_WorkcellDataSource.CurrentUser,
                Enum_WorkcellDataSource.CurrentPeopleName,
                Enum_WorkcellDataSource.CurrentPeopleDepartment,
                Enum_WorkcellDataSource.CurrentDate,
                Enum_WorkcellDataSource.CurrentTime,
                Enum_WorkcellDataSource.CurrentDateTime,
                Enum_WorkcellDataSource.CurrentPeopleOrg,
                Enum_WorkcellDataSource.CurrentPeopleDeptAndPos,
                Enum_WorkcellDataSource.CurrentPeoplePosition,
                Enum_WorkcellDataSource.CurrentPeopleManageOrg,
                Enum_WorkcellDataSource.CurrentPeopleDepartmentID,
                Enum_WorkcellDataSource.CurrentPeopleDepartmentShortName,
                Enum_WorkcellDataSource.CurrentPeopleEmail
            };
            return readonlySource.Contains(source);
        }

        #endregion

        #region 行列转成WorkCellID
        public static string ConvertToWorkcellId(int row, int col)
        {
            return ((char) (col + 65 - 1)).ToString() + row.ToString();
        }
        #endregion

        #region 获取NodeField

        public static Dictionary<string, int> GetNodeFieldDict(string mid, string nid)
        {
            var doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
            List<NodeField> nodeFields = mgr.FindNodeField("MessageID=@MessageID AND NodeKey=@NodeKey AND DocId=@DocId", new Dictionary<string, object>()
            {
                {
                    "@MessageID",
                    mid
                },
                {
                    "@NodeKey",
                    nid
                },
                {
                    "@DocId",
                    doc.DocID
                }
            }, "");
            return nodeFields.ToDictionary(n => n.FieldKey, n => n.Control);
        }
        #endregion

        #region 获得Cell的值
        /*
         * 获得Cell的值
         * 目前支持Text及Attachment类型
         * 
         * dataSource:
         * 0 Text
         * 1 MultiUserText
         * 10 Attachment
         */
        public static object GetCellValue(Worksheet worksheet, int row, int col, int dataSource)
        {
            object cellValue = null;

            switch (dataSource)
            {
                case 0:
                    {
                        var workcell = worksheet.GetWorkcell(row, col);
                        cellValue = workcell == null ? string.Empty : workcell.WorkcellValue;
                    }
                    break;
                case 1:
                    {
                        List<object> deptHistoryInfo = new List<object>();

                        var workcellValue = worksheet.GetWorkcell(row, col);
                        XmlNodeList deptHistory = workcellValue.History;
                        foreach (XmlNode node in deptHistory)
                        {
                            var emplId = node.Attributes.GetNamedItem("emplId").InnerText;
                            var value = node.Attributes.GetNamedItem("Value").InnerText;
                            var updateTime = node.Attributes.GetNamedItem("UpdateTime").InnerText;
                            var historyDeptInfo = new
                            {
                                user = orgMgr.GetEmployee(emplId).EmplName,
                                text = value,
                                time = updateTime
                            };
                            deptHistoryInfo.Add(historyDeptInfo);
                        }

                        cellValue = deptHistoryInfo;
                    }
                    break;
                case 10:
                    {
                        var workcell = worksheet.GetWorkcell(row, col);
                        if (workcell == null)
                        {
                            throw new Exception(string.Format("{0},{1}值为NULL", row, col));
                        }
                        else
                        {
                            var attachments = new List<object>();
                            var internalAttachs = workcell.WorkcellInternalValue.Split(';').ToList();
                            foreach (var attachId in internalAttachs)
                            {
                                if (string.IsNullOrEmpty(attachId))
                                {
                                    continue;
                                }
                                else
                                {
                                    var attachment = mgr.GetAttachment(attachId);
                                    attachments.Add(attachment);
                                }
                            }
                            cellValue = attachments;
                        }
                    }
                    break;
                default:
                    break;
            }

            return cellValue;
        }
        #endregion

        #region 根据用户获得对应映射表
        public static List<string> GetTablesByUser(string emplID)
        {
            return new List<string>() { "XYD_ReceiveFile" };
        }
        #endregion

        #region 检查用户是否在角色中
        /// <summary>
        /// 检查用户是否在对应角色组中
        /// </summary>
        /// <param name="EmplID"></param>
        /// <param name="GroupName"></param>
        /// <returns></returns>
        public static bool CheckInGroup(string EmplID, string GroupName)
        {
            List<Role> RoleList = orgMgr.FindRoleForEmplID(EmplID);

            var role = RoleList.Where(n => n.RoleName == GroupName).FirstOrDefault();

            return (role != null);
        }
        #endregion

        #region 启动子流程
        public static Node StartSubflow(Node baseNode, SubflowConfig subflowConfig, Employee employee, string handlerEmplId)
        {
            // 发起用户的ID
            var currentEmplId = employee.EmplID;

            Message theMessage = mgr.StartWorkflow(subflowConfig.SubflowId, currentEmplId, (HttpContext)null);
            theMessage.DataSourceID = baseNode.Message.ToString() + ":" + baseNode.NodeKey;
            theMessage.MessageStatus = 1;
            theMessage.MessageIssuedDept = employee.DeptID;
            mgr.UpdateMessage(theMessage);
            SheetMgr sheetMgr = new SheetMgr();
            foreach (SubflowMapping subflowMapping in subflowConfig.MappingOut)
            {
                string[] strArray1 = subflowMapping.From.Split('.');
                string[] strArray2 = subflowMapping.To.Split('.');
                Doc docByName1 = mgr.GetDocByName(baseNode.MessageID, strArray1[0]);
                Worksheet worksheet1 = sheetMgr.GetWorksheet(docByName1.DocHelperID);
                Doc docByName2 = mgr.GetDocByName(theMessage.MessageID, strArray2[0]);
                Worksheet worksheet2 = sheetMgr.GetWorksheet(docByName2.DocHelperID);
                Workcell workcell = worksheet1.GetWorkcell(strArray1[1]);
                worksheet2.SetCellValue(strArray2[1], workcell.WorkcellValue, workcell.WorkcellInternalValue);
                worksheet2.Save();
            }
            string initNodeKey = theMessage.InitNodeKey;
            if (!(subflowConfig.StartAtNode != theMessage.InitNodeKey))
            {
                initNodeKey = subflowConfig.StartAtNode;
            }
            Node node = mgr.GetNode(theMessage.MessageID, initNodeKey);
            BECommand beCommand = new BECommand("update WKF_MessageHandle set UserID=@p1 where MessageID=@p2 and NodeKey=@p3 and UserID=@p4");
            beCommand.SetParameters("@p1", (object)handlerEmplId);
            beCommand.SetParameters("@p2", (object)theMessage.MessageID);
            beCommand.SetParameters("@p3", (object)initNodeKey);
            beCommand.SetParameters("@p4", (object)currentEmplId);
            beCommand.ExecuteNonQuery();
            beCommand.Close();
            mgr.AddWorkflowHistory(new WorkflowHistory()
            {
                MessageID = theMessage.MessageID,
                NodeKey = "",
                NodeName = "[启动]",
                HandledBy = currentEmplId,
                HandledTime = DateTime.Now,
                DelegateTo = "",
                ProcType = 0,
                Note = string.Format("由“{0}”的节点“{1}”启动子流程", (object)baseNode.Message.MessageTitle, (object)baseNode.NodeName)
            });
            return node;
        }
        #endregion

        #region 获得公文预警日期
        public static DateTime? GetMessageAlarmDate(Worksheet worksheet, AlarmValue config)
        {
            /*
             * 变量定义
             */
            // 预警日期
            DateTime alarmDate;

            // 获取预警日期
            var cellAlarmDate = GetCellValue(worksheet, config.row, config.col, 0);
            if (string.IsNullOrEmpty(Convert.ToString(cellAlarmDate)))
            {
                return null;
            }
            else
            {
                // 解析日期格式
                if (!DateTime.TryParse(Convert.ToString(cellAlarmDate), out alarmDate))
                {
                    throw new Exception(string.Format("日期{0}格式不正确", cellAlarmDate));
                }
                else
                {
                    return alarmDate;
                }
            }
        }
        #endregion

        #region 极光推送
        /// <summary>
        /// 极光推送
        /// </summary>
        /// <param name="targets">推送对象数组</param>
        /// <param name="mid">推送数据
        /// {"message":xxx}
        /// </param>
        public static void SendJPushNotification(List<string> targets, Dictionary<string, object> data)
        {
            /*
             * 推送数据解析
             */
            // 消息标题
            var Title = data["title"];
            // 消息类型
            var Type = data["type"];
            // 消息内容
            var content = data["content"];

            // 极光推送AppKey与MasterSecrect获取
            string JPushAppKey = System.Configuration.ConfigurationManager.AppSettings["JPushAppKey"];
            string JPushSecrect = System.Configuration.ConfigurationManager.AppSettings["JPushSecrect"];
            JPushClient PushClient = new JPushClient(JPushAppKey, JPushSecrect);
            Jiguang.JPush.Model.PushPayload PayLoad = new Jiguang.JPush.Model.PushPayload();
            PayLoad.Platform = "all";
            PayLoad.Audience = new Dictionary<string, object>()
            {
                {"alias", targets }
            };
            PayLoad.Notification = new Jiguang.JPush.Model.Notification()
            {
                Alert = data["title"].ToString(),
                Android = new Jiguang.JPush.Model.Android()
                {
                    Extras = new Dictionary<string, object>()
                    {
                        {"type", Type },
                        {"content", content }
                    }
                },
                IOS = new Jiguang.JPush.Model.IOS
                {
                    Badge = "+1",
                    Extras = new Dictionary<string, object>()
                    {
                        {"type", Type },
                        {"content", content }
                    }
                }
            };
            PayLoad.Options = new Jiguang.JPush.Model.Options
            {
                IsApnsProduction = true // 设置 iOS 推送生产环境。不设置默认为开发环境。
            };
            PushClient.SendPushAsync(PayLoad.ToString());
        }
        #endregion

        #region 推送流程预警通知
        public static void SendMessageAlarmNotification(string mid)
        {
            /*
             * 变量定义
             */
            // 推送数据
            Dictionary<string, object> dict = new Dictionary<string, object>();

            /*
             * 根据消息ID获得对应的标题
             */
            Message message = mgr.GetMessage(mid);
            string filter1 = "MessageID=@mid and HandleStatus !=0";
            Dictionary<string, object> paramList1 = new Dictionary<string, object>();
            paramList1.Add("@mid", (object)mid);

            List<string> targets = mgr.FindMessageHandle(filter1, paramList1, string.Empty).Select(n => n.UserID).ToList();

            // 系统内通知
            //foreach (var emplID in targets)
            //{
            //    var data = JsonConvert.SerializeObject(new
            //    {
            //        MessageID = mid,
            //        NewWin = true,
            //        Url = "/Apps/Workflow/Running/Open?mid=" + mid
            //    });
            //    notifyMgr.SendNotification("DEP", emplID.ToString(), string.Format("您有新的流程预警提醒 \"{0}\"", message.MessageTitle), data);
            //}

            // 极光通知
            dict["type"] = DEP_Constants.JPush_Workflow_Type;
            dict["title"] = string.Format("您有新的流程预警提醒 \"{0}\"", message.MessageTitle);
            dict["content"] = new Dictionary<string, object>()
                {
                    {"mid",  mid}
                };
            SendJPushNotification(targets, dict);
        }
        #endregion

        #region 获得工作流模版列表
        public static List<XYD_Template_Entity> GetTemplates()
        {
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", "templates"));

            using (StreamReader sr = new StreamReader(filePathName))
            {
                var config = JsonConvert.DeserializeObject<XYD_Templates>(sr.ReadToEnd());
                return config.workflows;
            }
        }
        #endregion

        #region 根据用户运行权限获得可使用的工作流模版
        public static List<Message> GetTemplatesByUser(string name)
        {
            List<Message> TemplateList = new List<Message>();

            //List<Folder> folder1 = mgr.FindFolder("", (Dictionary<string, object>)null, "FolderName");
            //for (int index = folder1.Count - 1; index >= 0; --index)
            //{
            //    if (!orgMgr.VerifyPermission(folder1[index].FolderID, name, "user", "view"))
            //        folder1.RemoveAt(index);
            //}
            //List<object> objectList1 = new List<object>();
            List<Message> templates = mgr.FindTemplates("", "", true, "MessageTitle");
            List<object> objectList2 = new List<object>();
            foreach (Message message in templates)
            {
                if (orgMgr.VerifyPermission(message.MessageID, name, "user", "run"))
                {
                    TemplateList.Add(message);
                }
            }
            return TemplateList;
        }
        #endregion

        #region 根据用户view权限获得工作流模版
        public static object GetViewTemplatesByUser(string name)
        {
            List<Message> TemplateList = new List<Message>();

            List<Folder> folder1 = mgr.FindFolder("", (Dictionary<string, object>)null, "FolderName");
            for (int index = folder1.Count - 1; index >= 0; --index)
            {
                if (!orgMgr.VerifyPermission(folder1[index].FolderID, name, "user", "view"))
                    folder1.RemoveAt(index);
            }
            Dictionary<string, List<Message>> resultDict = new Dictionary<string, List<Message>>();
            foreach (Folder folder2 in folder1)
            {
                List<Message> templates = mgr.FindTemplates(folder2.FolderID, "", true, "MessageTitle");
                resultDict[folder2.FolderName] = templates;
            }
            var resultList = resultDict.Select(n => new { Area = n.Key, Value = n.Value.Select(m => new { ID = m.MessageID, Title = m.MessageTitle}).ToList() }).ToList();
            return resultList;
        }
        #endregion

        #region 获得表单详情
        public static XYD_Fields GetWorkflowFields(string emplId, string NodeId, string MessageID)
        {
            Message message = mgr.GetMessage(MessageID);
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(MessageID));
            Worksheet worksheet = doc.Worksheet;
            var templateId = message.FromTemplate;
            var filePathName = GetConfigPath(templateId, message.MessageID, DEP_Constants.Config_Type_Start);

            using (StreamReader sr = new StreamReader(filePathName))
            {
                var fields = JsonConvert.DeserializeObject<XYD_Fields>(sr.ReadToEnd(), new XYDCellJsonConverter());

                for (int i = 0; i < fields.Fields.Count; i++)
                {
                    XYD_Base_Cell cell = fields.Fields[i];
                    fields.Fields[i] = FillCellValue(emplId, NodeId, MessageID, worksheet, cell);
                }
                return fields;
            }
        }
        #endregion

        #region 填充物品采购表单
        public static void FillApplyGoods(string MessageID, string goods)
        {
            List<string> goodsArray = goods.Split(';').ToList();
            Message message = mgr.GetMessage(MessageID);
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(MessageID));
            Worksheet worksheet = doc.Worksheet;
            XYD_Array_Cell arrayCell = null;

            var defaultVersion = GetDefaultConfigVersion(message.FromTemplate);
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], message.FromTemplate, defaultVersion, "start.json");
            using (StreamReader sr = new StreamReader(filePathName))
            {
                var fields = JsonConvert.DeserializeObject<XYD_Fields>(sr.ReadToEnd(), new XYDCellJsonConverter());

                foreach (XYD_Base_Cell cell in fields.Fields)
                {
                    if (cell.Type == 3)
                    {
                        arrayCell = (XYD_Array_Cell)cell;
                    }
                }
                if (arrayCell == null)
                {
                    throw new Exception("未找到物品表格");
                }
                if (arrayCell.Array.Count < goodsArray.Count)
                {
                    throw new Exception("申领物品不能选择超过5种");
                }
                // 填充到对应的里面
                var updateCells = new List<Workcell>();
                for (int i = 0; i < goodsArray.Count; i++)
                {
                    var rowArray = goodsArray.ElementAt(i).Split(',');
                    List<XYD_Cell_Value> rowCells = arrayCell.Array.ElementAt(i);
                    // 物品名称
                    XYD_Cell_Value goodsCell = rowCells.ElementAt(0);
                    var cell = worksheet.GetWorkcell(goodsCell.Row, goodsCell.Col);
                    cell.WorkcellValue = rowArray.ElementAt(0);
                    updateCells.Add(cell);
                    // 型号
                    XYD_Cell_Value modelCell = rowCells.ElementAt(1);
                    var cell2 = worksheet.GetWorkcell(modelCell.Row, modelCell.Col);
                    cell2.WorkcellValue = rowArray.ElementAt(1);
                    updateCells.Add(cell2);
                    // 单位
                    XYD_Cell_Value unitCell = rowCells.ElementAt(3);
                    var cell3 = worksheet.GetWorkcell(unitCell.Row, unitCell.Col);
                    cell3.WorkcellValue = rowArray.ElementAt(2);
                    updateCells.Add(cell3);
                }
                worksheet.UpdateWorkcells(updateCells);
            }
        }
        #endregion

        #region 确认发起表单
        public static void ConfirmStartWorkflow(string MessageID, string jsonString)
        {
            Message message = mgr.GetMessage(MessageID);
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(MessageID));
            Worksheet worksheet = sheetMgr.GetWorksheet(doc.DocHelperID);

            var fields = JsonConvert.DeserializeObject<XYD_Fields>(jsonString, new XYDCellJsonConverter());
            List<Workcell> workCellList = new List<Workcell>();
            foreach (XYD_Base_Cell cell in fields.Fields)
            {
                XYD_Single_Cell singleCell = null;
                XYD_Array_Cell arrayCell = null;
                if (cell.Type == 0)
                {
                    singleCell = (XYD_Single_Cell)cell;
                    var workcell = AssignWorkCell(worksheet, singleCell.Value);
                    if (workcell != null)
                    {
                        workCellList.Add(workcell);
                    }
                }
                else if (cell.Type == 3)
                {
                    arrayCell = (XYD_Array_Cell)cell;
                    foreach (List<XYD_Cell_Value> rowCells in arrayCell.Array)
                    {
                        foreach (XYD_Cell_Value innerCell in rowCells)
                        {
                            var workcell = AssignWorkCell(worksheet, innerCell);
                            if (workcell != null)
                            {
                                workCellList.Add(workcell);
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("不支持的类型");
                }
            }
            // 使用SetCellValue可以更新InternalValue，不要使用UpdateWorkCells
            foreach(var workCell in workCellList)
            {
                worksheet.SetCellValue(workCell.WorkcellRow, workCell.WorkcellCol, workCell.WorkcellValue, workCell.WorkcellInternalValue);
            }
            sheetMgr.UpdateWorksheet(worksheet);
        }
        #endregion

        #region 表单数据赋值Worksheet

        public static Workcell AssignWorkCell(Worksheet worksheet, XYD_Cell_Value innerCell)
        {
            var workcell = worksheet.GetWorkcell(innerCell.Row, innerCell.Col);
            if (workcell != null && innerCell.Type != 10)
            {
                workcell.WorkcellValue = innerCell.Value;
                workcell.WorkcellInternalValue = innerCell.InterValue;
                return workcell;
            }

            return null;
        }
        #endregion

        #region 签批申请
        public static void AuditMessage(string mid, string nid, string operate, string opinion)
        {
            XYD_Audit_Node auditNode = null;
            var message = mgr.GetMessage(mid);
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
            Worksheet worksheet = doc.Worksheet;
            var templateId = message.FromTemplate;
            var filePathName = GetConfigPath(templateId, message.MessageID, DEP_Constants.Config_Type_Audit);
            using (StreamReader sr = new StreamReader(filePathName))
            {
                var nodes = JsonConvert.DeserializeObject<XYD_Audit>(sr.ReadToEnd(), new XYDCellJsonConverter());
                foreach (XYD_Audit_Node node in nodes.Nodes)
                {
                    if (node.NodeID == nid)
                    {
                        auditNode = node;
                    }
                }
                if (auditNode == null)
                {
                    throw new Exception("未找到节点对应审批配置");
                }
                // 开始签批
                UpdateCell(worksheet, auditNode.Operate.Row, auditNode.Operate.Col, operate, string.Empty);
                UpdateCell(worksheet, auditNode.Opinion.Row, auditNode.Opinion.Col, opinion, string.Empty);
            }
        }

        public static void UpdateCell(Worksheet worksheet, int row, int col, string value, string interValue)
        {
            Workcell cell = worksheet.GetWorkcell(row, col);
            if (cell == null)
            {
                throw new Exception(string.Format("对应单元格{0},{1}不存在", row, col));
            }
            cell.WorkcellValue = value;
            cell.WorkcellInternalValue = interValue;
            worksheet.UpdateWorkcells(new List<Workcell> { cell });
        }
        #endregion

        #region 获得签批节点
        public static XYD_Audit_Node GetAuditNode(string mid, string nid)
        {
            XYD_Audit_Node auditNode = null;
            var message = mgr.GetMessage(mid);
            var templateId = message.FromTemplate;
            var filePathName = GetConfigPath(templateId, message.MessageID, DEP_Constants.Config_Type_Audit);
            using (StreamReader sr = new StreamReader(filePathName))
            {
                var nodes = JsonConvert.DeserializeObject<XYD_Audit>(sr.ReadToEnd(), new XYDCellJsonConverter());
                foreach (XYD_Audit_Node node in nodes.Nodes)
                {
                    if (node.NodeID == nid)
                    {
                        auditNode = node;
                    }
                }
                return auditNode;
            }
        }
        #endregion

        #region 获得申请编号
        public static string GenerateSerialNumber(string name)
        {
            var year = Convert.ToString(DateTime.Now.Year);
            using (var db = new DefaultConnection())
            {
                var entity = db.SerialNo.Where(n => n.Name == name).FirstOrDefault();
                if (entity != null)
                {
                    if (!year.Contains(Convert.ToString(entity.Year)))
                    {
                        entity.Year += 1;
                        entity.Number = 1;
                        db.SaveChanges();
                    }
                    var num = string.Empty;
                    if (entity.Number < 10)
                    {
                        num = string.Format("000{0}", entity.Number);
                    }
                    else if (entity.Number < 100)
                    {
                        num = string.Format("00{0}", entity.Number);
                    }
                    else if (entity.Number < 1000)
                    {
                        num = string.Format("0{0}", entity.Number);
                    }
                    var serialNo = string.Format("{0}-{1}", DateTime.Now.ToString("yyyyMMdd"), num);
                    return serialNo;
                }
                else
                {
                    throw new Exception("没有找到对应编号配置");
                }
            }
        }
        #endregion

        #region 填充事务编号
        public static void FillSerialNumber(string mid)
        {
            XYD_Serial config = null;
            var message = mgr.GetMessage(mid);
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
            Worksheet worksheet = doc.Worksheet;
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], "serialNumber.json");

            using (StreamReader sr = new StreamReader(filePathName))
            {
                var serials = JsonConvert.DeserializeObject<XYD_Serials>(sr.ReadToEnd(), new XYDCellJsonConverter());

                foreach (XYD_Serial serialConfig in serials.Serials)
                {
                    if (serialConfig.FromId == message.FromTemplate)
                    {
                        config = serialConfig;
                    }
                }
            }
            if (config == null)
            {
                throw new Exception("未找到对应事务位置");
            }
            else
            {
                Workcell cell = worksheet.GetWorkcell(config.SnPos.Row, config.SnPos.Col);
                string serialNo = WorkflowUtil.GenerateSerialNumber(message.FromTemplate);
                cell.WorkcellValue = serialNo;
                worksheet.UpdateWorkcells(new List<Workcell> { cell });
            }
        }
        #endregion

        #region 抽取流程中的编号
        public static string ExtractSerialNumber(string mid)
        {
            XYD_Serial config = null;
            var message = mgr.GetMessage(mid);
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
            Worksheet worksheet = doc.Worksheet;
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], "serialNumber.json");

            using (StreamReader sr = new StreamReader(filePathName))
            {
                var serials = JsonConvert.DeserializeObject<XYD_Serials>(sr.ReadToEnd(), new XYDCellJsonConverter());

                foreach (XYD_Serial serialConfig in serials.Serials)
                {
                    if (serialConfig.FromId == message.FromTemplate)
                    {
                        config = serialConfig;
                    }
                }
            }
            if (config == null)
            {
                throw new Exception("未找到对应事务位置");
            }
            else
            {
                Workcell cell = worksheet.GetWorkcell(config.SnPos.Row, config.SnPos.Col);
                return cell.WorkcellValue;
            }
        }
        #endregion

        #region 根据目标流程ID获取源头ID
        public static XYD_Serial GetSourceSerial(string mid)
        {
            XYD_Serial config = null;
            var message = mgr.GetMessage(mid);
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
            Worksheet worksheet = doc.Worksheet;
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], "serialNumber.json");

            using (StreamReader sr = new StreamReader(filePathName))
            {
                var serials = JsonConvert.DeserializeObject<XYD_Serials>(sr.ReadToEnd(), new XYDCellJsonConverter());

                foreach (XYD_Serial serialConfig in serials.Serials)
                {
                    if (serialConfig.ToId == message.FromTemplate)
                    {
                        config = serialConfig;
                    }
                }
            }
            if (config == null)
            {
                throw new Exception("未找到对应事务位置");
            }
            else
            {
                return config;
            }
        }
        #endregion

        #region 根据Mapping规则映射两表中数据
        public static void MappingBetweenFlows(string sourceMessageId, string destMessageId, List<SubflowMapping> MappingOut)
        {
            SheetMgr sheetMgr = new SheetMgr();
            foreach (SubflowMapping subflowMapping in MappingOut)
            {
                string[] strArray1 = subflowMapping.From.Split('.');
                string[] strArray2 = subflowMapping.To.Split('.');
                Doc docByName1 = mgr.GetDocByName(sourceMessageId, strArray1[0]);
                Worksheet worksheet1 = sheetMgr.GetWorksheet(docByName1.DocHelperID);
                Doc docByName2 = mgr.GetDocByName(destMessageId, strArray2[0]);
                Worksheet worksheet2 = sheetMgr.GetWorksheet(docByName2.DocHelperID);
                Workcell workcell = worksheet1.GetWorkcell(strArray1[1]);
                worksheet2.SetCellValue(strArray2[1], workcell.WorkcellValue, workcell.WorkcellInternalValue);
                worksheet2.Save();
            }
        }
        #endregion

        #region 记录流程操作记录
        public static void AddWorkflowHistory(string EmplID, string NodeName, string MessageID, string Operation, string Opinion)
        {
            using (var db = new DefaultConnection())
            {
                var history = new XYD_Audit_Record();
                history.EmplID = EmplID;
                history.NodeName = NodeName;
                history.MessageID = MessageID;
                history.Operation = Operation;
                history.Opinion = Opinion;
                history.CreateTime = DateTime.Now;
                history.UpdateTime = DateTime.Now;
                db.Audit_Record.Add(history);
                db.SaveChanges();
            }
        }
        #endregion

        #region 获得最新审批记录
        public static string GetLatestOpinion(string EmplID, string MessageID)
        {
            using (var db = new DefaultConnection())
            {
                var record = db.Audit_Record.Where(n => n.MessageID == MessageID && n.EmplID == EmplID).OrderByDescending(n => n.CreateTime).FirstOrDefault();
                return record == null ? string.Empty : string.Format("您已{0}了该申请", record.Operation);
            }
        }
        #endregion

        #region 获得节点事件
        public static XYD_Event GetCellEvent(string mid, int row, int col)
        {
            /*
             * 根据模板ID获得对应的配置
             */
            Dictionary<string, object> dict = new Dictionary<string, object>();

            XYD_Event resultEvent = null;

            Message message = mgr.GetMessage(mid);
            var templateID = message.FromTemplate;
            var filePathName = GetConfigPath(templateID, mid, DEP_Constants.Config_Type_Event);
            using (StreamReader sr = new StreamReader(filePathName))
            {
                var config = JsonConvert.DeserializeObject<XYD_EventCells>(sr.ReadToEnd());

                foreach (var eventCell in config.cells)
                {
                    if (eventCell.Row == row && eventCell.Col == col)
                    {
                        resultEvent = eventCell;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }

                return resultEvent;
            }
        }
        #endregion
        
        #region 根据用户职位、城市、时常获得住宿标准
        public static int GetHotelStandard(string emplId, string city, int hour)
        {
            // 未满24小时，没有住宿补贴
            if (hour < 24)
            {
                return 0;
            }
            // 获取城市等级
            var cityLevel = GetCityLevel(city);
            // 获取用户角色
            var userRole = GetRoleById(emplId);

            // 一线城市
            if (cityLevel == DEP_Constants.First_Tier_City)
            {
                // 副总经理
                if (userRole == DEP_Constants.ViceCEO)
                {
                    return 600;
                }
                else if (userRole == DEP_Constants.DeptManager)
                {
                    return 500;
                }
                else
                {
                    return 300;
                }
            }
            // 二线城市
            else if (cityLevel == DEP_Constants.Second_Tier_City)
            {
                // 副总经理
                if (userRole == DEP_Constants.ViceCEO)
                {
                    return 500;
                }
                else if (userRole == DEP_Constants.DeptManager)
                {
                    return 400;
                }
                else
                {
                    return 300;
                }
            }
            // 三线城市
            else if (cityLevel == DEP_Constants.Third_Tier_City)
            {
                // 副总经理
                if (userRole == DEP_Constants.ViceCEO)
                {
                    return 350;
                }
                else if (userRole == DEP_Constants.DeptManager)
                {
                    return 300;
                }
                else
                {
                    return 250;
                }
            }
            // 国外，50美金/日
            else
            {
                return 350;
            }
        }
        #endregion

        #region 获取城市级别
        /// <summary>
        /// 获取城市级别
        /// </summary>
        /// <param name="city"></param>
        /// <returns>
        /// 1:一线城市（含直辖市）
        /// 2：省会城市
        /// 3：省辖市、县级市及以下
        /// 4：国外
        /// </returns>
        public static int GetCityLevel(string city)
        {
            // 一线城市
            var firstCity = System.Configuration.ConfigurationManager.AppSettings["FirstCity"];
            // 二线城市
            var secondCity = System.Configuration.ConfigurationManager.AppSettings["SecondCity"];
            var firstCityArray = firstCity.Split(',').Select(n => n.Trim()).ToList();
            var secondCityArray = secondCity.Split(',').Select(n => n.Trim()).ToList();
            // 检查是否是一线城市
            if (firstCityArray.Any(city.Contains))
            {
                return DEP_Constants.First_Tier_City;
            }
            // 检查是否是二线城市
            if (secondCityArray.Any(city.Contains))
            {
                return DEP_Constants.Second_Tier_City;
            }
            // 检查是否是国外
            if (Regex.IsMatch(city, "^[a-zA-Z0-9\\s]*$"))
            {
                return DEP_Constants.Aboard_City;
            }
            // 默认三线
            return DEP_Constants.Third_Tier_City;
        }
        #endregion

        #region 获取用户职位级别
        /// <summary>
        /// 
        /// </summary>
        /// <param name="emplId"></param>
        /// <returns>
        /// 副总经理、部门经理、员工
        /// </returns>
        public static int GetRoleById(string emplId)
        {
            // 副总经理
            var viceCEO = System.Configuration.ConfigurationManager.AppSettings["ViceCEO"];
            // 部门经理
            var deptManager = System.Configuration.ConfigurationManager.AppSettings["DeptManager"];
            // 员工
            var staff = System.Configuration.ConfigurationManager.AppSettings["Staff"];

            if (OrgUtil.CheckRole(emplId, viceCEO))
            {
                return DEP_Constants.ViceCEO;
            }
            else if (OrgUtil.CheckRole(emplId, deptManager))
            {
                return DEP_Constants.DeptManager;
            }
            else
            {
                return DEP_Constants.Staff;
            }

        }
        #endregion

        #region 根据位置获取对应单元格的值
        public static string GetFieldValue(List<XYD_Base_Cell> fields, string cellId)
        {
            // 单元格值
            string resultValue = null;

            // 提取CellId中的Row、Col
            var cellPosArray = cellId.Split('-').ToList();
            int Row = int.Parse(cellPosArray.ElementAt(1));
            int Col = int.Parse(cellPosArray.ElementAt(2));

            List<Workcell> workCellList = new List<Workcell>();
            foreach (XYD_Base_Cell cell in fields)
            {
                XYD_Single_Cell singleCell = null;
                XYD_Array_Cell arrayCell = null;
                if (cell.Type == 0)
                {
                    singleCell = (XYD_Single_Cell)cell;
                    if (singleCell.Value.Row == Row && singleCell.Value.Col == Col)
                    {
                        resultValue = singleCell.Value.Value;
                    }
                }
                else if (cell.Type == 3)
                {
                    arrayCell = (XYD_Array_Cell)cell;
                    foreach (List<XYD_Cell_Value> rowCells in arrayCell.Array)
                    {
                        foreach (XYD_Cell_Value innerCell in rowCells)
                        {
                            if (innerCell.Row == Row && innerCell.Col == Col)
                            {
                                resultValue = innerCell.Value;
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("不支持的类型");
                }
            }
            if (resultValue == null)
            {
                throw new Exception("未找到对应单元格");
            }
            return resultValue;
        }
        #endregion

        #region 根据CellID获取行列
        public static DEP_CellPos GetCellPos(string cellId)
        {
            // 提取CellId中的Row、Col
            var cellPos = new DEP_CellPos();
            var cellPosArray = cellId.Split('-').ToList();
            cellPos.row = int.Parse(cellPosArray.ElementAt(1));
            cellPos.col = int.Parse(cellPosArray.ElementAt(2));
            return cellPos;
        }
        #endregion

        #region 根据mid获得worksheet
        public static Worksheet GetWorksheet(string mid)
        {
            Message message = mgr.GetMessage(mid);
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
            Worksheet worksheet = doc.Worksheet;
            return worksheet;
        }
        #endregion

        #region 更新流程页面数据
        public static XYD_Cell_Value GetFieldsCellValue(List<XYD_Base_Cell> fields, int row, int col)
        {
            foreach (XYD_Base_Cell cell in fields)
            {
                XYD_Single_Cell singleCell = null;
                XYD_Array_Cell arrayCell = null;
                if (cell.Type == 0)
                {
                    singleCell = (XYD_Single_Cell)cell;
                    if (singleCell.Value.Row == row && singleCell.Value.Col == col)
                    {
                        return singleCell.Value;
                    }
                }
                else if (cell.Type == 3)
                {
                    arrayCell = (XYD_Array_Cell)cell;
                    foreach (List<XYD_Cell_Value> rowCells in arrayCell.Array)
                    {
                        foreach (XYD_Cell_Value innerCell in rowCells)
                        {
                            if (innerCell.Row == row && innerCell.Col == col)
                            {
                                return innerCell;
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("不支持的类型");
                }
            }
            throw new Exception("未找到对应单元格");
        }
        #endregion

        #region 更新流程页面数据
        public static void UpdateFieldsCellValue(List<XYD_Base_Cell> fields, XYD_Cell_Value value)
        {
            foreach (XYD_Base_Cell cell in fields)
            {
                XYD_Single_Cell singleCell = null;
                XYD_Array_Cell arrayCell = null;
                if (cell.Type == 0)
                {
                    singleCell = (XYD_Single_Cell)cell;
                    if (singleCell.Value.Row == value.Row && singleCell.Value.Col == value.Col)
                    {
                        singleCell.Value = value;
                        break;
                    }
                }
                else if (cell.Type == 3)
                {
                    arrayCell = (XYD_Array_Cell)cell;
                    foreach (List<XYD_Cell_Value> rowCells in arrayCell.Array)
                    {
                        for (int i=0; i< rowCells.Count; i++)
                        {
                            XYD_Cell_Value innerCell = rowCells[i];
                            if (innerCell.Row == value.Row && innerCell.Col == value.Col)
                            {
                                innerCell = value;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("不支持的类型");
                }
            }
        }
        #endregion

        #region 根据流程和类别，获得科目
        public static XYD_SubVoucherCode GetSubVoucherCode(string mid, string name)
        {
            XYD_SubVoucherCode SubCode = null;
            var workflowId = string.Empty;
            if (mid == DEP_Constants.INVOICE_WORKFLOW_ID)
            {
                workflowId = DEP_Constants.INVOICE_WORKFLOW_ID;
            }
            else
            {
                Message message = mgr.GetMessage(mid);
                workflowId = message.FromTemplate;
            }

            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("Voucher.json"));

            using (StreamReader sr = new StreamReader(filePathName))
            {
                var config = JsonConvert.DeserializeObject<XYD_VoucherCodes>(sr.ReadToEnd());
                foreach (var VoucherCode in config.VoucherCodes)
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
                            foreach (var Code in VoucherCode.Codes)
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
                return SubCode;
            }
        }
        #endregion

        #region 根据流程ID获取配置版本
        public static string GetConfigVersion(string templetId, string mid)
        {
            var checkSql = string.Format(@"SELECT Version FROM XYD_ReceiveFile WHERE MessageId = '{0}'", mid);
            var checkResultList = DbUtil.ExecuteSqlCommand(checkSql, DbUtil.GetReceiveFile).ToList();
            if (checkResultList.Count == 0)
            {
                return GetDefaultConfigVersion(templetId);
            }
            else
            {
                return checkResultList.First().ToString();
            }
        }
        #endregion

        #region 获取最新流程配置版本
        public static string GetDefaultConfigVersion(string templateId)
        {
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", "version"));
            using (StreamReader sr = new StreamReader(filePathName))
            {
                var configVersion = JsonConvert.DeserializeObject<XYD_Config_Version>(sr.ReadToEnd());
                foreach (XYD_Version version in configVersion.versions)
                {
                    if (version.WorkflowId == templateId)
                    {
                        return version.Version;
                    }
                }
                throw new Exception("流程对应版本不存在");
            }
        }
        #endregion

        #region 获得科目类型列表
        public static XYD_VoucherOptions GetVoucherOptions()
        {
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", "voucherOptions"));
            using (StreamReader sr = new StreamReader(filePathName))
            {
                var Credits = JsonConvert.DeserializeObject<XYD_VoucherOptions>(sr.ReadToEnd());
                return Credits;
            }
        }
        #endregion

        #region 判断是否是付款申请
        public static bool IsPayWorkflow(string mid)
        {
            var message = mgr.GetMessage(mid);
            if (message.FromTemplate == "84f434ac-4626-4748-b294-fe94b16b953c" || message.FromTemplate == "06d9b887-ca03-4042-8202-2aafaeaa3634")
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}