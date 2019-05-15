using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Common;
using Appkiz.Library.Security;
using DeptOA.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace DeptOA.Common
{
    public class WorkflowUtil
    {
        /*
         * 变量定义
         */
        private static OrgMgr orgMgr = new OrgMgr();
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
            /*
             * 根据模板ID获得对应的配置
             */
            Message message = mgr.GetMessage(mid);
            var templateID = message.FromTemplate;

            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", templateID));
            using (StreamReader sr = new StreamReader(filePathName))
            {
                var config = JsonConvert.DeserializeObject<DEP_Node>(sr.ReadToEnd());

                return config.table;
            }
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

            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", "global"));
            using (StreamReader sr = new StreamReader(filePathName))
            {
                var tableList = new List<string>();
                var DeptRelation = JsonConvert.DeserializeObject<DeptFlowRelation>(sr.ReadToEnd());

                foreach (RelationItem item in DeptRelation.relations)
                {
                    if (deptList.Contains(item.dept))
                    {
                        tableList.Add(item.tableName);
                    }
                }

                return tableList;
            }
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
        public static Node StartSubflow(Node baseNode, SubflowConfig subflowConfig, string currentEmplId, string handlerEmplId)
        {
            Message theMessage = mgr.StartWorkflow(subflowConfig.SubflowId, currentEmplId, (HttpContext)null);
            theMessage.DataSourceID = baseNode.Message.ToString() + ":" + baseNode.NodeKey;
            theMessage.MessageStatus = 1;
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
    }
}