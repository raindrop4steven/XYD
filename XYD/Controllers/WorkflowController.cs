using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using XYD.Entity;
using XYD.Common;
using XYD.Models;
using XYD.Services;
using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using System.Text;
using Appkiz.Library.Security.Authentication;
using Appkiz.Library.Common;
using JUST;
using Newtonsoft.Json.Linq;
using Appkiz.Apps.Workflow.Web.Controllers;

namespace XYD.Controllers
{
    public class WorkflowController : Appkiz.Apps.Workflow.Web.Controllers.Controller
    {
        WorkflowMgr mgr = new WorkflowMgr();
        SheetMgr sheetMgr = new SheetMgr();
        OrgMgr orgMgr = new OrgMgr();

        #region 根据用户获得发起工作流模版
        [HttpPost]
        public ActionResult GetTemplates()
        {
            List<XYD_Template_Entity> myTempaltes = GetMyTemplates(User.Identity.Name);
            return this.Json((object)new
            {
                Succeed = true,
                Data = myTempaltes
            }, (JsonRequestBehavior)0);
        }

        // 获得我的工作流
        public List<XYD_Template_Entity> GetMyTemplates(string name)
        {
            // 模版列表
            List<XYD_Template_Entity> resultTemplates = new List<XYD_Template_Entity>();
            List<Message> TemplateList = WorkflowUtil.GetTemplatesByUser(name);
            List<XYD_Template_Entity> Templates = WorkflowUtil.GetTemplates();
            
            foreach (var template in TemplateList)
            {
                foreach (var templetEntity in Templates)
                {
                    if (templetEntity.Id == template.MessageID)
                    {
                        resultTemplates.Add(templetEntity);
                    }
                }
            }


            return resultTemplates.OrderBy(o => o.Order).ToList();
        }
        #endregion

        #region 发起流程
        [HttpGet]
        public ActionResult Open(string mid, string nid, string fieldValues)
        {
            Message message = mgr.GetMessage(mid);
            if (message == null)
                return (ActionResult)RedirectToAction("Error", (object)new
                {
                    err = RunningErrorCodes.INVALID_ID
                });
            if (message.IsTemplate == 1)
            {
                if (message.MessageStatus == -1)
                {
                    return ResponseUtil.Error("模版无效");
                }
                if (message.GetNodeList().Count == 0)
                {
                    return ResponseUtil.Error("模版无节点");
                }
                if (string.IsNullOrEmpty(message.InitNodeKey))
                {
                    return ResponseUtil.Error("初始节点为空");
                }
                Message theMessage = mgr.StartWorkflow(message.MessageID, User.Identity.Name, HttpContext.ApplicationInstance.Context);
                theMessage.MessageType = "temp";
                mgr.UpdateMessage(theMessage);
                if (theMessage == null)
                {
                    return ResponseUtil.Error("创建失败");
                } else
                {
                    return ResponseUtil.OK(new
                    {
                        MessageId = theMessage.MessageID,
                        WorkflowId = theMessage.FromTemplate,
                        WorksheetId = mgr.GetDocHelperIdByMessageId(theMessage.MessageID),
                        MessageTitle = theMessage.MessageTitle,
                        NodeId = theMessage.InitNodeKey
                    });
                }
            } else
            {
                return ResponseUtil.Error("参数不是工作流模版");
            }
        }
        #endregion

        #region 获得发起流程配置
        public ActionResult GetStartFields(string MessageID)
        {
            try
            {
                XYD_Fields fields = WorkflowUtil.GetStartFields(MessageID);
                return ResponseUtil.OK(fields);
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

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
                        // 判断是否是转发传阅节点
                        if (node.NodeKey.StartsWith(DEP_Constants.Transfer_Node_Key_Header) || node.NodeType == 5)
                        {
                            // node状态
                            node.NodeStatus = 3;
                            mgr.UpdateNode(node);
                            // 将Messagehandle中该数据清除掉
                            mgr.DelMessageHandle(MessageID, node.NodeKey, employee.EmplID);
                            // 添加到WorkflowHistory
                            var workflowHistory = new WorkflowHistory();
                            workflowHistory.MessageID = MessageID;
                            workflowHistory.NodeName = node.NodeType == 5 ? "(传阅)" : "(转发传阅)";
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
                var inputTypes = WorkflowUtil.GetNodeInputTypes(MessageID, NodeID);
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
                        action = control,
                        inputTypes = inputTypes
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
    }
}