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
using Appkiz.Library.Security.Authentication;
using Appkiz.Library.Common;

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
        public ActionResult MappingData()
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
            var MessageID = Request.Params["mid"];
            // 节点ID
            var NodeID = Request.Params["node"];

            try
            {
                /*
                 * 配置读取
                 */
                string tableName = WorkflowUtil.GetTableName(MessageID);
                DEP_Mapping mappings = WorkflowUtil.GetNodeMappings(MessageID, NodeID);

                // 判断是否存在对应配置
                if (mappings == null)
                {
                    return ResponseUtil.Error(string.Format("流程{0}节点{1}没有对应配置", MessageID, NodeID));
                }
                else
                {
                    bool runResult = wkfService.AddOrUpdateRecord(MessageID, tableName, mappings);

                    return ResponseUtil.OK(runResult);
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 移动端公文详情
        [HttpPost]
        public ActionResult GetDetailInfo(FormCollection collection)
        {
            /*
             * 变量定义
             */
            // 工作流Service
            WorkflowService wkfService = new WorkflowService();
            var employee = (User.Identity as AppkizIdentity).Employee;
            var NodeID = string.Empty;

            /*
             * 参数获取
             */
            // 消息ID
            var MessageID = collection["mid"];

            try
            {
                List<Node> source = mgr.ListNodeToBeHandle(employee.EmplID, "");
                foreach (Node node in source)
                {
                    if (node.MessageID == MessageID)
                    {
                        NodeID = node.NodeKey;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }

                /*
                 * 配置读取
                 */
                string tableName = WorkflowUtil.GetTableName(MessageID);
                List<DEP_Detail> details = WorkflowUtil.GetNodeDetail(MessageID);
                var action = WorkflowUtil.GetNodeAction(MessageID, NodeID);
                var control = WorkflowUtil.GetNodeControl(MessageID, NodeID);

                // 判断是否存在对应配置
                if (details == null)
                {
                    return ResponseUtil.Error(string.Format("流程{0}没有对应详情配置", MessageID));
                }
                else
                {
                    // 获取表单详情
                    var detail = wkfService.GetDetailInfo(MessageID, NodeID, details);

                    return ResponseUtil.OK(new
                    {
                        detail = detail,
                        control = action,
                        action = control
                    });
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 移动端公文详情
        [HttpPost]
        public ActionResult GetPageInfo(FormCollection collection)
        {
            /*
             * 变量定义
             */
            // 工作流Service
            WorkflowService wkfService = new WorkflowService();
            var employee = (User.Identity as AppkizIdentity).Employee;
            var NodeID = string.Empty;

            /*
             * 参数获取
             */
            // 消息ID
            var MessageID = collection["mid"];

            try
            {
                List<Node> source = mgr.ListNodeToBeHandle(employee.EmplID, "");
                foreach (Node node in source)
                {
                    if (node.MessageID == MessageID)
                    {
                        NodeID = node.NodeKey;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }

                /*
                 * 配置读取
                 */
                string tableName = WorkflowUtil.GetTableName(MessageID);
                List<DEP_Detail> details = WorkflowUtil.GetNodeDetail(MessageID);

                // 判断是否存在对应配置
                if (details == null)
                {
                    return ResponseUtil.Error(string.Format("流程{0}没有对应详情配置", MessageID));
                }
                else
                {
                    // 获取表单详情
                    var detail = wkfService.GetPageInfo(MessageID, NodeID, details);

                    return ResponseUtil.OK(new
                    {
                        detail = detail
                    });
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 启动子流程
        [HttpPost]
        public ActionResult StartSubflow(FormCollection collection)
        {
            /*
             * 变量定义
             */
            var employee = (User.Identity as AppkizIdentity).Employee;

            /*
             * 参数获取
             */
            // 消息ID
            var MessageID = collection["mid"];
            // 节点ID
            var NodeID = collection["nid"];
            // 子节点的处理人
            var HandlerEmplId = collection["handlerEmplId"];

            /*
             * 参数校验
             */

            /*
             * 启动新流程
             */
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", "735d333f-d695-4199-9b42-bc65e8ee6a33-subflow"));
            using (StreamReader sr = new StreamReader(filePathName))
            {
                SubflowConfig subflowConfig = JsonConvert.DeserializeObject<SubflowConfig>(sr.ReadToEnd());
                // 获得当前节点信息
                var node = mgr.GetNode(MessageID, NodeID);
                var retNode = WorkflowUtil.StartSubflow(node, subflowConfig, employee.EmplID, HandlerEmplId);

                return ResponseUtil.OK(new
                {
                    MessageID = retNode.MessageID,
                    NodeKey = retNode.NodeKey,
                    NewWin = true,
                    Url = ("/Apps/Workflow/Running/Open?mid=" + retNode.MessageID + "&nid=" + retNode.NodeKey)
                });
            }
        }

        
        #endregion

        #region 测试Json解析
        public ActionResult Config()
        {
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", "735d333f-d695-4199-9b42-bc65e8ee6a33"));
            using (StreamReader sr = new StreamReader(filePathName))
            {
                var mappingModel = JsonConvert.DeserializeObject<DEP_Node>(sr.ReadToEnd());

                return ResponseUtil.OK(mappingModel);
            }
        }
        #endregion

        #region 详情测试
        [HttpPost]
        public ActionResult ShowDetail(FormCollection collection)
        {
            /*
             * 参数获取
             */
            // 消息ID
            var MessageId = collection["mid"];

            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(MessageId));
            Worksheet worksheet = doc.Worksheet;
            Message message = mgr.GetMessage(MessageId);

            var deptInfo = WorkflowUtil.GetCellValue(worksheet, 13, 4, 1);

            return ResponseUtil.OK(new
            {
                deptInfo = deptInfo
            });
        }

        [HttpPost]
        public ActionResult TestCellID(FormCollection collection)
        {
            var CellID = collection["cellID"];

            int row, col;

            Worksheet worksheet = new Worksheet();
            worksheet.TranslateWorkcellName(CellID, out row, out col);

            return ResponseUtil.OK(new
            {
                row = row,
                col = col
            });
        }
        #endregion
    }
}