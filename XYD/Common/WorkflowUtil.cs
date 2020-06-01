using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Notification;
using Appkiz.Library.Security;
using XYD.Entity;
using Jiguang.JPush;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using XYD.Models;

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

        #region 根据流程ID获取对应版本配置路径
        /// <summary>
        /// 根据模版ID获取配置路径
        /// </summary>
        /// <param name="templateID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetConfigPath(string templateID, string mid, string type)
        {
            string version = GetConfigVersion(mid);
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
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], templateID, defaultVersion, "main.json");

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
        #endregion

        #region 获得节点动作
        public static Dictionary<string, object> GetNodeAction(string mid, string nid)
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
        #endregion

        #region 获得节点权限
        public static object GetNodeControl(string mid, string nid)
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
        #endregion

        #region 获得流程节点对应的输入控件类型
        public static object GetNodeInputTypes(string mid, string nid)
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

        #endregion

        #region 获得流程转换配置
        public static string GetAppTransformer(string mid)
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
        #endregion

        #region 填充cell的值
        public static void FillCellValue(string emplId, string NodeId, string MessageID, Worksheet worksheet, XYD_Base_Cell cell, bool canEdit)
        {
            Dictionary<string, int> nodeFeildDict = GetNodeFieldDict(MessageID, NodeId);
            XYD_Single_Cell singleCell = null;
            XYD_Array_Cell arrayCell = null;
            if (cell.Type == 0)
            {
                singleCell = (XYD_Single_Cell)cell;
                ParseInnerCell(emplId, NodeId, MessageID, worksheet, singleCell.Value, nodeFeildDict);
            }
            else if (cell.Type == 3)
            {
                arrayCell = (XYD_Array_Cell)cell;
                foreach (List<XYD_Cell_Value> rowCells in arrayCell.Array)
                {
                    for (int i= 0; i < rowCells.Count; i++)
                    {
                        XYD_Cell_Value innerCell = rowCells[i];
                        ParseInnerCell(emplId, NodeId, MessageID, worksheet, innerCell, nodeFeildDict);
                    }
                }
            }
            else
            {
                throw new Exception("不支持的类型");
            }
        }
        #endregion

        #region 解析内部Cell
        public static void ParseInnerCell(string emplId, string NodeId, string MessageID, Worksheet worksheet, 
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
                        
            innerCell = ReflectionUtil.ParseCellValue(emplId, NodeId, MessageID, innerCell);
            var workcellId = ConvertToWorkcellId(innerCell.Row, innerCell.Col);
            var canEdit = false;
            var required = false;
            if (NodeFieldDict.ContainsKey(workcellId))
            {
                int control = NodeFieldDict[workcellId];
                canEdit = true;
                required = control == 2; // 1:可空；2：必填
            }
            innerCell.CanEdit = canEdit;
            innerCell.Required = required;
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

            List<Folder> folder1 = mgr.FindFolder("", (Dictionary<string, object>)null, "FolderName");
            for (int index = folder1.Count - 1; index >= 0; --index)
            {
                if (!orgMgr.VerifyPermission(folder1[index].FolderID, name, "user", "view"))
                    folder1.RemoveAt(index);
            }
            List<object> objectList1 = new List<object>();
            foreach (Folder folder2 in folder1)
            {
                List<Message> templates = mgr.FindTemplates(folder2.FolderID, "", true, "MessageTitle");
                List<object> objectList2 = new List<object>();
                foreach (Message message in templates)
                {
                    //if (orgMgr.VerifyPermission(message.MessageID, name, "user", "run"))
                    //{
                    //    TemplateList.Add(message);
                    //}
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

        #region 获得发起流程的表单配置
        public static XYD_Fields GetStartFields(string emplId, string NodeId, string MessageID)
        {
            Message message = mgr.GetMessage(MessageID);
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(MessageID));
            Worksheet worksheet = doc.Worksheet;
            var templateId = message.FromTemplate;
            var defaultVersion = GetDefaultConfigVersion(templateId);
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], templateId, defaultVersion, "start.json");

            using (StreamReader sr = new StreamReader(filePathName))
            {
                var fields = JsonConvert.DeserializeObject<XYD_Fields>(sr.ReadToEnd(), new XYDCellJsonConverter());

                foreach (XYD_Base_Cell cell in fields.Fields)
                {
                    // 查找对应的值
                    FillCellValue(emplId, NodeId, MessageID, worksheet, cell, true);
                }
                return fields;
            }
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
            //var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}-start.json", message.FromTemplate));

            using (StreamReader sr = new StreamReader(filePathName))
            {
                var fields = JsonConvert.DeserializeObject<XYD_Fields>(sr.ReadToEnd(), new XYDCellJsonConverter());

                foreach (XYD_Base_Cell cell in fields.Fields)
                {
                    // 查找对应的值
                    FillCellValue(emplId, NodeId, MessageID, worksheet, cell, false);
                }
                return fields;
            }
        }
        #endregion

        #region 确认发起表单
        public static void ConfirmStartWorkflow(string MessageID, string jsonString)
        {
            Message message = mgr.GetMessage(MessageID);
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(MessageID));
            Worksheet worksheet = doc.Worksheet;

            var fields = JsonConvert.DeserializeObject<XYD_Fields>(jsonString, new XYDCellJsonConverter());
            List<Workcell> workCellList = new List<Workcell>();
            foreach (XYD_Base_Cell cell in fields.Fields)
            {
                XYD_Single_Cell singleCell = null;
                XYD_Array_Cell arrayCell = null;
                if (cell.Type == 0)
                {
                    singleCell = (XYD_Single_Cell)cell;
                    var workcell = worksheet.GetWorkcell(singleCell.Value.Row, singleCell.Value.Col);
                    if (workcell != null && singleCell.Value.Type != 10)
                    {
                        workcell.WorkcellValue = singleCell.Value.Value;
                        workcell.WorkcellInternalValue = singleCell.Value.InterValue;
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
                            var workcell = worksheet.GetWorkcell(innerCell.Row, innerCell.Col);
                            if (workcell != null && innerCell.Type != 10)
                            {
                                workcell.WorkcellValue = innerCell.Value;
                                workcell.WorkcellInternalValue = innerCell.InterValue;
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
            worksheet.UpdateWorkcells(workCellList);
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
            //var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}-audit.json", message.FromTemplate));
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
            //var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}-audit.json", message.FromTemplate));
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
            var defaultVersion = GetDefaultConfigVersion(templateID);
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], templateID, defaultVersion, "event.json");
            //var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}-event.json", templateID));

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

        #region 根据流程ID获取配置版本
        public static string GetConfigVersion(string mid)
        {
            var checkSql = string.Format(@"SELECT Version FROM XYD_ReceiveFile WHERE MessageId = '{0}'", mid);
            var checkResultList = DbUtil.ExecuteSqlCommand(checkSql, DbUtil.GetReceiveFile).ToList();
            if (checkResultList.Count == 0)
            {
                throw new Exception("流程不存在");
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
    }
}