using Appkiz.Apps.Workflow.Library;
using DeptOA.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace DeptOA.Common
{
    public class WorkflowUtil
    {
        /*
         * 变量定义
         */
        private static WorkflowMgr mgr = new WorkflowMgr();

        /*
         * 获得配置
         */
        public static DEP_NodeValue GetNodeConfig(string mid, string nid)
        {
            /*
             * 变量定义
             */
            // 节点配置
            DEP_NodeValue nodeConfig = null;

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
                foreach (var nodeAction in config.nodes)
                {
                    if (nodeAction.key == nid)
                    {
                        nodeConfig = nodeAction.value;
                    }
                    else
                    {
                        continue;
                    }
                }
                return nodeConfig;
            }
        }

        public static string GetTableName(string mid, string nid)
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
    }
}