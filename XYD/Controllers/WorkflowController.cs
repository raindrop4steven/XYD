using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using XYD.Entity;
using XYD.Common;
using XYD.Services;
using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security.Authentication;
using JUST;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace XYD.Controllers
{
    public class WorkflowController : Appkiz.Apps.Workflow.Web.Controllers.Controller
    {
        WorkflowMgr mgr = new WorkflowMgr();

        #region 根据用户获得发起工作流模版
        [Authorize]
        [HttpPost]
        public ActionResult GetTemplates()
        {
            List<XYD_Template_Entity> myTemplates = GetMyTemplates(User.Identity.Name);
            return ResponseUtil.OK(myTemplates);
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
        [Authorize]
        [HttpGet]
        public ActionResult Open(string mid, string nid, string fieldValues)
        {
            Message message = mgr.GetMessage(mid);
            if (message == null)
            {
                throw new Exception("模板ID无效");
            }
            if (message.IsTemplate == 1)
            {
                if (message.MessageStatus == -1)
                {
                    throw new Exception("模版无效");
                }
                if (message.GetNodeList().Count == 0)
                {
                    throw new Exception("模版无节点");
                }
                if (string.IsNullOrEmpty(message.InitNodeKey))
                {
                    throw new Exception("初始节点为空");
                }
                Message theMessage = mgr.StartWorkflow(message.MessageID, User.Identity.Name, HttpContext.ApplicationInstance.Context);
                theMessage.MessageType = "temp";
                mgr.UpdateMessage(theMessage);
                if (theMessage == null)
                {
                    throw new Exception("创建失败");
                }
                return ResponseUtil.OK(new
                {
                    MessageId = theMessage.MessageID,
                    WorkflowId = theMessage.FromTemplate,
                    WorksheetId = mgr.GetDocHelperIdByMessageId(theMessage.MessageID),
                    MessageTitle = theMessage.MessageTitle,
                    NodeId = theMessage.InitNodeKey
                });
            }
            else
            {
                throw new Exception("参数不是工作流模版");
            }
        }
        #endregion

        #region 获得发起流程配置
        [Authorize]
        public ActionResult GetStartFields(string NodeId, string MessageID)
        {
            var employee = (User.Identity as AppkizIdentity).Employee;

            XYD_Fields fields = WorkflowUtil.GetStartFields(employee.EmplID, NodeId, MessageID);
            return ResponseUtil.OK(fields);
        }
        #endregion

        #region 确认发起流程
        [Authorize]
        [HttpPost]
        public ActionResult ConfirmStartWorkflow(string MessageID)
        {
            Stream stream = Request.InputStream;
            stream.Seek(0, SeekOrigin.Begin);
            string json = new StreamReader(stream).ReadToEnd();
            WorkflowUtil.ConfirmStartWorkflow(MessageID, json);
            return ResponseUtil.OK("流程发起成功");
        }
        #endregion

        #region 审批同意/驳回
        [Authorize]
        [HttpPost]
        public ActionResult Audit(FormCollection collection)
        {
            var operate = string.Empty;
            var employee = (User.Identity as AppkizIdentity).Employee;
            var mid = collection["mid"];
            var nid = collection["nid"];
            var operateString = collection["operate"];
            var opinion = collection["opinion"];
            // 判断是否是再次发起，提醒到网页端处理
            if (nid == DEP_Constants.Start_Node_Key)
            {
                throw new Exception("被驳回的审批，请到网页端修改后再提交");
            }
            if (operateString == "0")
            {
                operate = "同意";
            }
            else
            {
                operate = "驳回";
            }
            WorkflowUtil.AuditMessage(mid, nid, operate, opinion);
            return ResponseUtil.OK("审批OK");
        }
        #endregion

        #region 添加流程处理记录
        public ActionResult AddAuditRecord(string mid, string node, string user)
        {
            // 变量定义
            var operate = string.Empty;
            var opinion = string.Empty;

            var message = mgr.GetMessage(mid);
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
            Worksheet worksheet = doc.Worksheet;
            Node currentNode = mgr.GetNode(mid, node);
            if (currentNode.NodeType == DEP_Constants.NODE_TYPE_START)
            {
                operate = DEP_Constants.Audit_Operate_Type_Start;
            }
            else if (currentNode.NodeType == DEP_Constants.NODE_TYPE_END)
            {
                operate = DEP_Constants.Audit_Operate_Type_End;
            }
            else
            {
                XYD_Audit_Node auditNode = WorkflowUtil.GetAuditNode(mid, node);
                if (auditNode == null)
                {
                    throw new Exception("没找到对应处理节点");
                }
                operate = worksheet.GetWorkcell(auditNode.Operate.Row, auditNode.Operate.Col).WorkcellValue;
                opinion = worksheet.GetWorkcell(auditNode.Opinion.Row, auditNode.Opinion.Col).WorkcellValue;
            }

            WorkflowUtil.AddWorkflowHistory(user, currentNode.NodeName, mid, operate, opinion);
            return ResponseUtil.OK("添加处理记录成功");
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

            /*
             * 配置读取
             */
            string tableName = WorkflowUtil.GetTableName(MessageID);
            DEP_Mapping mappings = WorkflowUtil.GetNodeMappings(MessageID, NodeID);

            // 判断是否存在对应配置
            if (mappings == null)
            {
                throw new Exception(string.Format("流程{0}节点{1}没有对应配置", MessageID, NodeID));
            }
            else
            {
                bool runResult = wkfService.AddOrUpdateRecord(MessageID, tableName, mappings);

                return ResponseUtil.OK(runResult);
            }
        }
        #endregion

        #region 移动端公文详情
        [Authorize]
        [HttpPost]
        public ActionResult GetDetailInfo(FormCollection collection)
        {
            /*
             * 变量定义
             */
            // 工作流Service
            WorkflowService wkfService = new WorkflowService();
            var NodeID = string.Empty;

            /*
             * 参数获取
             */
            // 消息ID
            var MessageID = collection["mid"];

            var employee = (User.Identity as AppkizIdentity).Employee;
                var isReadOnly = false;
                List<Node> source = mgr.ListNodeToBeHandle(employee.EmplID, "");
                foreach (Node node in source)
                {
                    if (node.MessageID == MessageID)
                    {
                        // 判断是否是转发传阅节点
                        if (node.NodeKey.StartsWith(DEP_Constants.Transfer_Node_Key_Header) || node.NodeType == 5)
                        {
                            isReadOnly = true;
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
                List<DEP_Detail> details = WorkflowUtil.GetNodeDetail(MessageID);
                var action = WorkflowUtil.GetNodeAction(MessageID, NodeID);
                var control = WorkflowUtil.GetNodeControl(MessageID, NodeID);
                var inputTypes = WorkflowUtil.GetNodeInputTypes(MessageID, NodeID);
                var transformer = WorkflowUtil.GetAppTransformer(MessageID);

                // 判断是否存在对应配置
                if (details == null)
                {
                    throw new Exception(string.Format("流程{0}没有对应详情配置", MessageID));
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
                        readOnly = isReadOnly,
                        detail = result,
                        control = action,
                        action = control,
                        inputTypes = inputTypes
                    });
                }
        }
        #endregion

        #region 移动端公文页面详情
        [Authorize]
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
            XYD_Fields fields = WorkflowUtil.GetWorkflowFields(employee.EmplID, NodeID, MessageID);
            var handle = wkfService.GetMessageHandle(MessageID);
            var history = wkfService.GetWorkflowHistory(MessageID);
            return ResponseUtil.OK(new
            {
                fields = fields,
                handle = handle,
                history = history
            });
        }
        #endregion

        #region Cell内容更新事件
        [Authorize]
        public ActionResult CellUpdateEvent()
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;

                Stream stream = Request.InputStream;
                stream.Seek(0, SeekOrigin.Begin);
                string json = new StreamReader(stream).ReadToEnd();
                var eventArguments = JsonConvert.DeserializeObject<XYD_Event_Argument>(json, new XYDCellJsonConverter());

                var eventConfig = WorkflowUtil.GetCellEvent(eventArguments.MessageId, eventArguments.CurrentCellValue.Row, eventArguments.CurrentCellValue.Col);
                var customFunc = ReflectionUtil.ParseCustomFunc(eventConfig.Event);
                // 事件方法前2个参数固定为【当前用户ID】和EventArgument
                var resultArguments = new List<object>();
                resultArguments.Add(employee.EmplID);
                resultArguments.Add(eventArguments);
                foreach (var arg in customFunc.ArgumentsArray)
                {
                    // 区分一下正常参数和Cell参数吧，不知道会不会用到
                    if (arg.StartsWith("#"))
                    {
                        var fieldValue = WorkflowUtil.GetFieldValue(eventArguments.Fields, arg);
                        resultArguments.Add(fieldValue);
                    }
                    else
                    {
                        resultArguments.Add(arg);
                    }
                }
                var result = ReflectionUtil.caller(customFunc.ClassName, customFunc.MethodName, resultArguments);
                return ResponseUtil.OK(result);
            }
            catch (TargetInvocationException e)
            {
                return ResponseUtil.Error(e.InnerException?.Message);
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 网页端流程处理记录
        public ActionResult ShowHistory(string mid)
        {
            var sql = string.Format(@"SELECT
	                                            a.NodeName,
	                                            b.EmplName,
	                                            a.Operation,
	                                            a.Opinion,
	                                            a.CreateTime
                                            FROM
	                                            XYD_Audit_Record a
	                                            INNER JOIN ORG_Employee b ON a.EmplID = b.EmplID
                                            WHERE a.MessageID = '{0}'", mid);
            var results = DbUtil.ExecuteSqlCommand(sql, DbUtil.GetHistory);
            return ResponseUtil.OK(new
            {
                history = results
            });
        }
        #endregion
    }
}