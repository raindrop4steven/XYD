using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using DeptOA.Entity;
using DeptOA.Common;
using DeptOA.Services;
using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using System.Text;

namespace DeptOA.Controllers
{
    public class WorkflowController : Controller
    {
        WorkflowMgr mgr = new WorkflowMgr();
        OrgMgr orgMgr = new OrgMgr();

        #region 展示部门指派页面
        public ActionResult ShowDepts()
        {
            return PartialView("GetDeptPeople");
        }
        #endregion

        #region 映射数据
        [HttpPost]
        public ActionResult MappingData(FormCollection collection)
        {
            /*
             * 变量定义
             */
            // 工作流Service
            WorkflowService wkfService = new WorkflowService();

            /*
             * 参数获取
             */
            // 消息ID
            var MessageID = collection["mid"];
            // 节点ID
            var NodeID = collection["nid"];

            try
            {
                /*
                 * 配置读取
                 */
                string tableName = WorkflowUtil.GetTableName(MessageID, NodeID);
                DEP_NodeValue nodeConfig = WorkflowUtil.GetNodeConfig(MessageID, NodeID);

                // 判断是否存在对应配置
                if (nodeConfig == null)
                {
                    return ResponseUtil.Error(string.Format("流程{0}节点{1}没有对应配置", MessageID, NodeID));
                }
                else
                {
                    bool runResult = wkfService.AddOrUpdateRecord(MessageID, tableName, nodeConfig);

                    return ResponseUtil.OK(runResult);
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 列表展示

        #endregion

        #region 测试Json解析
        public ActionResult Config()
        {
            using (StreamReader sr = new StreamReader(Server.MapPath("~/mapping.json")))
            {
                var mappingModel = JsonConvert.DeserializeObject<DEP_Node>(sr.ReadToEnd());

                return ResponseUtil.OK(mappingModel);
            }
        }
        #endregion
    }
}