using Appkiz.Apps.Workflow.Library;
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

        /*
         * 获得配置
         */
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

        /// <summary>
        /// 获得详情配置
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
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
                                EmplId = emplId,
                                EmplName = orgMgr.GetEmployee(emplId).EmplName,
                                Value = value,
                                UpdateTime = updateTime
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
    }
}