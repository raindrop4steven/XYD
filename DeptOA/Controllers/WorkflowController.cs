using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using DeptOA.Entity;
using DeptOA.Common;
using DeptOA.Models;
using DeptOA.Services;
using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using System.Text;
using Appkiz.Library.Security.Authentication;
using Appkiz.Library.Common;
using JUST;
using Newtonsoft.Json.Linq;

namespace DeptOA.Controllers
{
    public class WorkflowController : Controller
    {
        WorkflowMgr mgr = new WorkflowMgr();
        OrgMgr orgMgr = new OrgMgr();

        #region 根据用户判断是否有部门表单配置
        public ActionResult CheckHasDept()
        {
            /*
             * 变量定义
             */
            // 当前用户
            var employee = (User.Identity as AppkizIdentity).Employee;

            /*
             * 检查当前用户是否有对应的表单配置，如果没有，则表明该用户没有被配置部门权限。
             * 如果有，则表明该用户有对应的部门权限。
             */
            // 根据当前用户获取对应的映射表
            var tableList = WorkflowUtil.GetTablesByUser(employee.EmplID);

            return ResponseUtil.OK(new
            {
                haveDeptConfig = (tableList.Count > 0)
            });
        }
        #endregion

        #region 判断流程属于部门还是党办
        [HttpPost]
        public ActionResult IsDeptWorkflow(FormCollection collection)
        {
            /*
             * 变量定义
             */
            // 工作流service
            WorkflowService wkfService = new WorkflowService();

            /*
             * 参数获取
             */
            // 消息ID
            var mid = collection["mid"];

            /*
             * 参数校验
             */
            // 消息ID
            if (string.IsNullOrEmpty(mid))
            {
                return ResponseUtil.Error("消息ID不能为空");
            }
            else
            {
                bool isDeptWorkflow = wkfService.IsDeptWorkflow(mid);

                return ResponseUtil.OK(new {
                    isDeptWorkflow = isDeptWorkflow
                });
            }
        }
        #endregion

        #region 展示部门指派页面
        public ActionResult ShowDepts()
        {
            return PartialView("GetDeptPeople");
        }
        #endregion

        #region 展示公文详情页面
        public ActionResult ShowDetailPage(string mid)
        {
            /*
             * 参数校验
             */
            // 消息ID
            if (string.IsNullOrEmpty(mid))
            {
                return ResponseUtil.Error("消息ID不能为空");
            }

            /*
             * 渲染详情页面
             */
            return PartialView("DetailPage");
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
                        // 判断是否是传阅节点
                        if (node.NodeKey.StartsWith(DEP_Constants.Transfer_Node_Key_Header))
                        {
                            // node状态
                            node.NodeStatus = 3;
                            mgr.UpdateNode(node);
                            // 将Messagehandle中该数据清除掉
                            mgr.DelMessageHandle(MessageID, node.NodeKey, employee.EmplID);
                            // 添加到WorkflowHistory
                            var workflowHistory = new WorkflowHistory();
                            workflowHistory.MessageID = MessageID;
                            workflowHistory.NodeName = "(转发传阅)";
                            workflowHistory.NodeKey = node.NodeKey;
                            workflowHistory.HandledBy = employee.EmplID;
                            workflowHistory.HandledTime = DateTime.Now;
                            workflowHistory.ProcType = 3;
                            mgr.AddWorkflowHistory(workflowHistory);
                        }
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
                var transformer = WorkflowUtil.GetAppTransformer(MessageID);

                // 判断是否存在对应配置
                if (details == null)
                {
                    return ResponseUtil.Error(string.Format("流程{0}没有对应详情配置", MessageID));
                }
                else
                {
                    // 获取表单详情
                    var detail = wkfService.GetDetailInfo(MessageID, NodeID, details);
                    var stringDetail = JsonConvert.SerializeObject(detail);
                    string transformedString = JsonTransformer.Transform(transformer, stringDetail);

                    JObject result = JObject.Parse(transformedString);

                    return ResponseUtil.OK(new
                    {
                        detail = result,
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

        #region 移动端公文页面详情
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
                var transformer = WorkflowUtil.GetWebTransformer(MessageID);

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
                    var detail = wkfService.GetDetailInfo(MessageID, NodeID, details);
                    var stringDetail = JsonConvert.SerializeObject(detail);
                    string transformedString = JsonTransformer.Transform(transformer, stringDetail);

                    JObject result = JObject.Parse(transformedString);
                    return ResponseUtil.OK(new
                    {
                        detail = result
                    });
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 获取个人意见配置
        [HttpPost]
        public ActionResult GetPrivateOpinionConfig(FormCollection collection)
        {
            /*
             * 变量定义
             */
            // 工作流Service
            var employee = (User.Identity as AppkizIdentity).Employee;

            /*
             * 参数获取
             */
            // 消息ID
            var MessageID = collection["mid"];

            var config = WorkflowUtil.GetPrivateOpinionConfig(MessageID);

            return ResponseUtil.OK(config);
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
            var HandlerEmplId = employee.EmplID;

            /*
             * 参数校验
             */
            // 消息ID
            if (string.IsNullOrEmpty(MessageID))
            {
                return ResponseUtil.Error("消息ID不能为空");
            }
            // 节点ID
            if (string.IsNullOrEmpty(NodeID))
            {
                return ResponseUtil.Error("节点ID不能为空");
            }
            // 子节点处理人
            if (string.IsNullOrEmpty(HandlerEmplId))
            {
                return ResponseUtil.Error("子节点处理人不能为空");
            }

            /*
             * 获得消息
             */
            var message = mgr.GetMessage(MessageID);

            /*
             * 根据当前用户部门获得对应的子节点流程配置
             */
            var subflowList = WorkflowUtil.GetSubflowByUser(employee.EmplID);
            if (subflowList.Count == 0)
            {
                return ResponseUtil.Error("当前用户没有子流程配置信息");
            }
            else
            {
                /*
                 * 判断当前流程是否已经启动了子流程，如果有则直接跳转，否则启动新流程
                 */
                using (var db = new DefaultConnection())
                {
                    var subflowRelation = db.SubflowRelation.Where(n => n.OriginMessageID == MessageID).FirstOrDefault();
                    if (subflowRelation == null)
                    {
                        // 直接创建子流程
                        var subflow = subflowList.ElementAtOrDefault(0);
                        SubflowConfig subflowConfig = WorkflowUtil.GetSubflowConfig(subflow, message.FromTemplate);
                        if (subflowConfig == null)
                        {
                            return ResponseUtil.Error("当前用户没有子流程配置信息");
                        }
                        else
                        {
                            // 获得当前节点信息
                            var node = mgr.GetNode(MessageID, NodeID);
                            var retNode = WorkflowUtil.StartSubflow(node, subflowConfig, employee, HandlerEmplId);

                            // 将流程对应关系存放到数据库
                            subflowRelation = new DEP_SubflowRelation();
                            subflowRelation.OriginMessageID = MessageID;
                            subflowRelation.SubflowMessageID = retNode.MessageID;
                            subflowRelation.CreateTime = DateTime.Now;
                            subflowRelation.UpdateTime = DateTime.Now;

                            db.SubflowRelation.Add(subflowRelation);
                            db.SaveChanges();

                            return ResponseUtil.OK(new
                            {
                                MessageID = retNode.MessageID,
                                NodeKey = retNode.NodeKey,
                                NewWin = true,
                                Url = ("/Apps/Workflow/Running/Open?mid=" + retNode.MessageID + "&nid=" + retNode.NodeKey)
                            });
                        }
                    }
                    else
                    {
                        // 返回已经创建的子流程
                        return ResponseUtil.OK(new
                        {
                            MessageID = subflowRelation.SubflowMessageID,
                            NodeKey = string.Empty,
                            NewWin = true,
                            Url = ("/Apps/Workflow/Running/Open?mid=" + subflowRelation.SubflowMessageID)
                        });
                    }
                }
            }
        }
        #endregion

        #region 判断当前用户是否可以发起子流程
        public ActionResult CheckSubflowPerm()
        {
            /*
             * 变量定义
             */
            var employee = (User.Identity as AppkizIdentity).Employee;

            /*
             * 判断用户是否属于文电管理员
             */
            // 获得部门领导组
            string DeptWendianGroup = System.Configuration.ConfigurationManager.AppSettings["DeptWendianGroup"];

            var havePermission = WorkflowUtil.CheckInGroup(employee.EmplID, DeptWendianGroup);

            return ResponseUtil.OK(new
            {
                havePermission = havePermission
            });
        }
        #endregion

        #region 添加或更新预警数据
        public ActionResult MappingAlarm()
        {
            try
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
                var mid = Request.Params["mid"];

                /*
                 * 参数校验
                 */
                // 消息ID
                if (string.IsNullOrEmpty(mid))
                {
                    return ResponseUtil.Error("消息ID不能为空");
                }

                /*
                 * 处理预警日期存储
                 */
                // 获得预警配置
                var alarmConfig = WorkflowUtil.GetMessageAlarmConfig(mid);

                if (alarmConfig == null)
                {
                    return ResponseUtil.Error(string.Format("流程{0}没有对应预警配置", mid));
                }
                else
                {
                    // 更新或添加预警信息
                    var retValue = wkfService.AddOrUpdateAlarm(mid, alarmConfig);

                    if (retValue)
                    {
                        return ResponseUtil.OK("工作流预警配置成功");
                    }
                    else
                    {
                        return ResponseUtil.Error("工作流预警配置失败");
                    }
                }
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 获取部门流程对应的父流程
        [HttpPost]
        public ActionResult GetOriginWorkflow(FormCollection collection)
        {
            /*
             * 变量定义
             */
            // 原地址
            object OriginWorkflowId;

            /*
             * 参数获取
             */
            // 消息ID
            var mid = collection["mid"];

            /*
             * 参数校验
             */
            // 消息ID
            if (string.IsNullOrEmpty(mid))
            {
                return ResponseUtil.Error("消息ID不能为空");
            }

            /*
             * 获取对应的原流程ID
             */
            using (var db = new DefaultConnection())
            {
                var relation = db.SubflowRelation.Where(n => n.SubflowMessageID == mid).FirstOrDefault();
                if (relation != null)
                {
                    OriginWorkflowId = relation.OriginMessageID;
                }
                else
                {
                    OriginWorkflowId = null;
                }

                return ResponseUtil.OK(new
                {
                    workflowId = "/Apps/Workflow/Running/Open?mid=" + OriginWorkflowId
                });
            }
        }
        #endregion

    }
}