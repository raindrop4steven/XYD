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

            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", templateID));

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

                var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", templateID));

                using (StreamReader sr = new StreamReader(filePathName))
                {
                    var config = JsonConvert.DeserializeObject<DEP_Node>(sr.ReadToEnd());

                    return config.values.details;
                }
            }
            catch(Exception e)
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

                var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", templateID));

                using (StreamReader sr = new StreamReader(filePathName))
                {
                    var config = JsonConvert.DeserializeObject<DEP_Node>(sr.ReadToEnd());

                    foreach(var action in config.values.actions)
                    {
                        if(action.key == nid)
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
                        foreach(var actionField in resultAction.value)
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

                var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", templateID));

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

                var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", templateID));

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

                var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}-app-transformer.json", templateID));

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

        #region 获得公文个人意见配置
        public static object GetPrivateOpinionConfig(string mid)
        {
            try
            {
                /*
                 * 根据模板ID获得对应的配置
                 */
                Dictionary<string, object> dict = new Dictionary<string, object>();

                Message message = mgr.GetMessage(mid);
                var templateID = message.FromTemplate;

                var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}-opinion.json", templateID));

                using (StreamReader sr = new StreamReader(filePathName))
                {
                    var config = JsonConvert.DeserializeObject<DEP_PrivateOpinions>(sr.ReadToEnd());

                    return config;
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 获得流程web转换配置
        public static string GetWebTransformer(string mid)
        {
            try
            {
                /*
                 * 根据模板ID获得对应的配置
                 */
                Dictionary<string, object> dict = new Dictionary<string, object>();

                Message message = mgr.GetMessage(mid);
                var templateID = message.FromTemplate;

                var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}-web-transformer.json", templateID));

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

            switch(dataSource)
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
                        if(workcell == null)
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
        public static List<string>GetTablesByUser(string emplID)
        {
            return new List<string>() { "XYD_ReceiveFile" };
        }
        #endregion

        #region 根据用户获得对应子流程
        public static List<string> GetSubflowByUser(string emplID)
        {
            var sql = string.Format(@"SELECT
	                                    DISTINCT(DeptID)
                                    FROM
	                                    ORG_Department a 
                                    WHERE
	                                    a.DeptHierarchyCode IN (
	                                    SELECT SUBSTRING
		                                    ( DeptHierarchyCode, 1, 5 )
	                                    FROM
		                                    ORG_Department b 
                                    WHERE
	                                    b.DeptID IN ( SELECT DeptID FROM ORG_EmplDept c WHERE c.EmplID = '{0}' ))", emplID);
            var deptList = DbUtil.ExecuteSqlCommand(sql, DbUtil.GetWorkflowByUser);

            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", "subflow"));
            using (StreamReader sr = new StreamReader(filePathName))
            {
                var subflowList = new List<string>();
                var DeptSubflowRelation = JsonConvert.DeserializeObject<DeptSubflowRelatoin>(sr.ReadToEnd());

                foreach (Subflow item in DeptSubflowRelation.subflows)
                {
                    if (deptList.Contains(item.dept))
                    {
                        subflowList.Add(item.subflowId);
                    }
                }

                return subflowList;
            }
        }
        #endregion

        #region 根据用户获得所在部门所有流程
        public static List<string> GetWorkflowsByUser(string emplID)
        {
            var sql = string.Format(@"SELECT
	                                    DISTINCT(DeptID)
                                    FROM
	                                    ORG_Department a 
                                    WHERE
	                                    a.DeptHierarchyCode IN (
	                                    SELECT SUBSTRING
		                                    ( DeptHierarchyCode, 1, 5 )
	                                    FROM
		                                    ORG_Department b 
                                    WHERE
	                                    b.DeptID IN ( SELECT DeptID FROM ORG_EmplDept c WHERE c.EmplID = '{0}' ))", emplID);
            var deptList = DbUtil.ExecuteSqlCommand(sql, DbUtil.GetWorkflowByUser);

            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", "workflow"));
            using (StreamReader sr = new StreamReader(filePathName))
            {
                var workflowList = new List<string>();
                var workflows = JsonConvert.DeserializeObject<Workflows>(sr.ReadToEnd());

                foreach (Workflow item in workflows.workflows)
                {
                    if (deptList.Contains(item.dept))
                    {
                        workflowList.AddRange(item.flows);
                    }
                }

                return workflowList;
            }
        }
        #endregion

        #region 根据消息模板获得子流程配置
        public static SubflowConfig GetSubflowConfig(string subflow, string templateId)
        {
            SubflowConfig subflowConfig = null;

            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}-subflow.json", subflow));
            using (StreamReader sr = new StreamReader(filePathName))
            {
                Subflows subflows = JsonConvert.DeserializeObject<Subflows>(sr.ReadToEnd());
                foreach(SubWorkflowRelation relation in subflows.subflows)
                {
                    if (relation.TemplateId == templateId)
                    {
                        subflowConfig = relation.Config;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }

                return subflowConfig;
            }
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

        #region 获得所有的部门流程
        public static List<string> GetAllDeptWorkflows()
        {
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", "workflow"));
            using (StreamReader sr = new StreamReader(filePathName))
            {
                var workflowList = new List<string>();
                var workflows = JsonConvert.DeserializeObject<Workflows>(sr.ReadToEnd());

                foreach (Workflow item in workflows.workflows)
                {
                    workflowList.AddRange(item.flows);
                }

                return workflowList;
            }
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

        #region 获得公文对应预警配置
        public static AlarmValue GetMessageAlarmConfig(string mid)
        {
            try
            {
                /*
                 * 根据模板ID获得对应的配置
                 */

                AlarmValue alarmConfig = null;

                Message message = mgr.GetMessage(mid);
                var templateID = message.FromTemplate;

                var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", "alert"));

                using (StreamReader sr = new StreamReader(filePathName))
                {
                    var dep_alarms = JsonConvert.DeserializeObject<DEP_MessageAlarmConfig>(sr.ReadToEnd());
                    foreach (var config in dep_alarms.alarms.configs)
                    {
                        if (config.key == templateID)
                        {
                            alarmConfig = config.value;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    return alarmConfig;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 获取预警提前天数
        public static int GetAlarmMessageDays()
        {
            try
            {
                var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", "alert"));

                using (StreamReader sr = new StreamReader(filePathName))
                {
                    var dep_alarms = JsonConvert.DeserializeObject<DEP_MessageAlarmConfig>(sr.ReadToEnd());

                    return dep_alarms.alarms.days;
                }
            }
            catch(Exception e)
            {
                throw e;
            }
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

        #region 根据用户获得可使用的工作流模版
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
                    if (orgMgr.VerifyPermission(message.MessageID, name, "user", "run"))
                    {
                        TemplateList.Add(message);
                    }
                }
            }
            return TemplateList;
        }
        #endregion
    }
}