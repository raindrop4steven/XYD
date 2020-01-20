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
using XYD.Common;

namespace XYD.Controllers
{
    public class WorkflowController : Appkiz.Apps.Workflow.Web.Controllers.Controller
    {
        WorkflowMgr mgr = new WorkflowMgr();
        SheetMgr sheetMgr = new SheetMgr();
        OrgMgr orgMgr = new OrgMgr();

        #region 根据用户获得发起工作流模版
        [Authorize]
        [HttpPost]
        public ActionResult GetTemplates()
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                List<XYD_Template_Entity> myTempaltes = GetMyTemplates(User.Identity.Name);
                return this.Json((object)new
                {
                    Succeed = true,
                    Data = myTempaltes
                }, (JsonRequestBehavior)0);
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
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
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;

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
                    }
                    else
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
                }
                else
                {
                    return ResponseUtil.Error("参数不是工作流模版");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 获得发起流程配置
        [Authorize]
        public ActionResult GetStartFields(string MessageID)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;

                XYD_Fields fields = WorkflowUtil.GetStartFields(MessageID);
                return ResponseUtil.OK(fields);
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 确认发起流程
        [Authorize]
        [HttpPost]
        public ActionResult ConfirmStartWorkflow(string MessageID)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;

                Stream stream = Request.InputStream;
                stream.Seek(0, SeekOrigin.Begin);
                string json = new StreamReader(stream).ReadToEnd();
                WorkflowUtil.ConfirmStartWorkflow(MessageID, json);
                return ResponseUtil.OK("流程发起成功");
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 审批同意/驳回
        [Authorize]
        [HttpPost]
        public ActionResult Audit(FormCollection collection)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                var mid = collection["mid"];
                var nid = collection["nid"];
                var operate = collection["operate"];
                var opinion = collection["opinion"];
                WorkflowUtil.AuditMessage(mid, nid, operate, opinion);
                return ResponseUtil.OK("审批OK");
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 添加流程处理记录
        public ActionResult AddAuditRecord(string mid, string node, string user)
        {
            try
            {
                // 变量定义
                var operate = string.Empty;
                var opinion = string.Empty;

                var message = mgr.GetMessage(mid);
                Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
                Worksheet worksheet = doc.Worksheet;
                if (node == DEP_Constants.Start_Node_Key)
                {
                    operate = DEP_Constants.Audit_Operate_Type_Start;
                }
                else
                {
                    XYD_Audit_Node auditNode = WorkflowUtil.GetAuditNode(mid, node);
                    if (auditNode == null)
                    {
                        return ResponseUtil.Error("没找到对应处理节点");
                    }
                    operate = worksheet.GetWorkcell(auditNode.Operate.Row, auditNode.Operate.Col).WorkcellValue;
                    opinion = worksheet.GetWorkcell(auditNode.Opinion.Row, auditNode.Opinion.Col).WorkcellValue;
                }

                WorkflowUtil.AddWorkflowHistory(user, mid, operate, opinion);
                return ResponseUtil.OK("添加处理记录成功");
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
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

            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;

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
                XYD_Fields fields = WorkflowUtil.GetWorkflowFields(MessageID);
                var handle = wkfService.GetMessageHandle(MessageID);
                var history = wkfService.GetWorkflowHistory(MessageID);
                return ResponseUtil.OK(new
                {
                    fields = fields,
                    handle = handle,
                    history = history
                });
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 生成编号
        public ActionResult FillSerialNo(string mid)
        {
            WorkflowUtil.FillSerialNumber(mid);
            return ResponseUtil.OK("OK");
        }
        #endregion

        #region 更新编号
        public ActionResult UpdateSerialNo(string mid)
        {
            var message = mgr.GetMessage(mid);
            using (var db = new DefaultConnection())
            {
                var entity = db.SerialNo.Where(n => n.Name == message.FromTemplate).FirstOrDefault();
                if (entity == null)
                {
                    return ResponseUtil.Error("未找到对应编号配置");
                }
                else
                {
                    entity.Number += 1;
                    db.SaveChanges();
                    return ResponseUtil.OK("更新编号成功");
                }
            }
        }
        #endregion

        #region 映射事务编号数据
        public ActionResult MappingSerialNo(string mid)
        {
            var employee = (User.Identity as AppkizIdentity).Employee;
            var message = mgr.GetMessage(mid);
            string serialNumber = WorkflowUtil.ExtractSerialNumber(mid);
            using (var db = new DefaultConnection())
            {
                var record = db.SerialRecord.Where(n => n.MessageID == mid).FirstOrDefault();
                if (record == null)
                {
                    record = new XYD_Serial_Record();
                    record.ID = Guid.NewGuid().ToString();
                    record.EmplID = employee.EmplID;
                    record.MessageID = mid;
                    record.WorkflowID = message.FromTemplate;
                    record.Sn = serialNumber;
                    record.Used = false;
                    record.CreateTime = DateTime.Now;
                    record.UpdateTime = DateTime.Now;
                    db.SerialRecord.Add(record);
                    db.SaveChanges();
                    // 更新编号配置
                    var entity = db.SerialNo.Where(n => n.Name == message.FromTemplate).FirstOrDefault();
                    if (entity == null)
                    {
                        return ResponseUtil.Error("未找到对应编号配置");
                    }
                    else
                    {
                        entity.Number += 1;
                        db.SaveChanges();
                        return ResponseUtil.OK("更新编号成功");
                    }
                }
                else
                {
                    record.Sn = serialNumber;
                    db.SaveChanges();
                }
                return ResponseUtil.OK("记录事务编号成功");
            }
        }
        #endregion

        #region 根据流程ID获取对应的来源sn
        public ActionResult GetSourceSerial(string mid)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                XYD_Serial serial = WorkflowUtil.GetSourceSerial(mid);
                using (var db = new DefaultConnection())
                {
                    var records = db.SerialRecord.Where(n => n.WorkflowID == serial.FromId && n.Used == false && n.EmplID == employee.EmplID).OrderByDescending(n => n.CreateTime).ToList();
                    return ResponseUtil.OK(new
                    {
                        records = records
                    });
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 使用编号
        public ActionResult UseSerialNumber(string sn, string mid)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                XYD_Serial serial = WorkflowUtil.GetSourceSerial(mid);
                // 设置编号已使用
                using (var db = new DefaultConnection())
                {
                    var record = db.SerialRecord.Where(n => n.WorkflowID == serial.FromId && n.Used == false && n.EmplID == employee.EmplID && n.Sn == sn).FirstOrDefault();
                    if (record == null)
                    {
                        return ResponseUtil.Error("配置为空");
                    }
                    else
                    {
                        record.Used = true;
                        db.SaveChanges();
                        return ResponseUtil.OK("使用编号成功");
                    }
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 选择编号，映射对应数据到报销单中
        public ActionResult MappingSourceToDest(string sn, string mid)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                XYD_Serial serial = WorkflowUtil.GetSourceSerial(mid);
                // 设置编号已使用
                using (var db = new DefaultConnection())
                {
                    var record = db.SerialRecord.Where(n => n.WorkflowID == serial.FromId && n.Used == false && n.EmplID == employee.EmplID && n.Sn == sn).FirstOrDefault();
                    if (record == null)
                    {
                        return ResponseUtil.Error("配置为空");
                    }
                    WorkflowUtil.MappingBetweenFlows(record.MessageID, mid, serial.MappingOut);
                    XYD_Fields fields = WorkflowUtil.GetStartFields(mid);
                    return ResponseUtil.OK(fields);
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 映射选择的物品列表到物品申请单中
        [Authorize]
        public ActionResult MappingGoods(string mid, string goods)
        {
            try
            {
                var goodsArray = goods.Split(',').ToList();
                WorkflowUtil.FillApplyGoods(mid, goodsArray);
                return ResponseUtil.OK("产品映射成功");
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion
    }
}