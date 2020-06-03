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
using System.Reflection;
using Appkiz.Apps.Workflow.Web.Models;

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
        public ActionResult GetStartFields(string NodeId, string MessageID)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;

                XYD_Fields fields = WorkflowUtil.GetStartFields(employee.EmplID, NodeId, MessageID);
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
                var operate = string.Empty;
                var employee = (User.Identity as AppkizIdentity).Employee;
                var mid = collection["mid"];
                var nid = collection["nid"];
                var operateString = collection["operate"];
                var opinion = collection["opinion"];
                // 判断是否是再次发起，提醒到网页端处理
                if (nid == DEP_Constants.Start_Node_Key)
                {
                    return ResponseUtil.Error("被驳回的审批，请到网页端修改后再提交");
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
                        return ResponseUtil.Error("没找到对应处理节点");
                    }
                    operate = worksheet.GetWorkcell(auditNode.Operate.Row, auditNode.Operate.Col).WorkcellValue;
                    opinion = worksheet.GetWorkcell(auditNode.Opinion.Row, auditNode.Opinion.Col).WorkcellValue;
                }

                WorkflowUtil.AddWorkflowHistory(user, currentNode.NodeName, mid, operate, opinion);
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
                        readOnly = isReadOnly,
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
        public ActionResult MappingSerialNo(string mid, string user)
        {
            var employee = orgMgr.GetEmployee(user);
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
                    bool isBaoxiaoRole = OrgUtil.CheckBaoxiaoUser(employee.EmplID);
                    var query = db.SerialRecord.Where(n => n.WorkflowID == serial.FromId && n.Used == false);
                    if (!isBaoxiaoRole)
                    {
                        query = query.Where(n => n.EmplID == employee.EmplID);
                    }
                    var records = query.OrderByDescending(n => n.CreateTime).ToList().Where(m => mgr.GetMessage(m.MessageID).MessageStatus == 2);
                    if (isBaoxiaoRole)
                    {
                        foreach(var record in records)
                        {
                            var user = orgMgr.GetEmployee(record.EmplID).EmplName;
                            var Sn = string.Format("{0} {1}", user, record.Sn);
                            record.Sn = Sn;
                        }
                    }
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
        public ActionResult UseSerialNumber(string sn, string mid, string user)
        {
            try
            {
                var employee = orgMgr.GetEmployee(user);
                if (OrgUtil.CheckCEO(employee.EmplID))
                {
                    return ResponseUtil.OK("总经理没有编号");
                }
                XYD_Serial serial = WorkflowUtil.GetSourceSerial(mid);
                // 设置编号已使用
                using (var db = new DefaultConnection())
                {
                    if (sn.Contains(" "))
                    {
                        var snArray = sn.Split(' ').ToList();
                        employee = orgMgr.FindEmployee("EmplName=@EmplName", new System.Collections.Hashtable()
                          {
                            {
                              "@EmplName",
                              snArray[0]
                            }
                          }, string.Empty, 0, 1).FirstOrDefault();
                        sn = snArray[1];
                    }
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
        public ActionResult MappingSourceToDest(string sn, string mid, int row = 0, int col = 0)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                XYD_Serial serial = WorkflowUtil.GetSourceSerial(mid);
                // 设置编号已使用
                using (var db = new DefaultConnection())
                {
                    if (sn.Contains(" "))
                    {
                        var snArray = sn.Split(' ');
                        employee = orgMgr.FindEmployee("EmplName=@EmplName", new System.Collections.Hashtable()
                          {
                            {
                              "@EmplName",
                              snArray[0]
                            }
                          }, string.Empty, 0, 1).FirstOrDefault();
                        sn = snArray[1];
                    }
                    var record = db.SerialRecord.Where(n => n.WorkflowID == serial.FromId && n.Used == false && n.EmplID == employee.EmplID && n.Sn == sn).FirstOrDefault();
                    if (record == null)
                    {
                        return ResponseUtil.Error("没有找到对应申请记录");
                    }
                    WorkflowUtil.MappingBetweenFlows(record.MessageID, mid, serial.MappingOut);
                    // 填充表单编号
                    if (row > 0 && col > 0)
                    {
                        Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
                        Worksheet worksheet = doc.Worksheet;
                        worksheet.SetCellValue(row, col, sn, string.Empty);
                        worksheet.Save();
                    }
                    XYD_Fields fields = WorkflowUtil.GetStartFields(employee.EmplID, DEP_Constants.Start_Node_Key, mid);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mid"></param>
        /// <param name="goods">资产名称1,型号1,单位1;资产名称2,型号2,单位2</param>
        /// <returns></returns>
        [Authorize]
        public ActionResult MappingGoods(string mid, string goods)
        {
            try
            {
                WorkflowUtil.FillApplyGoods(mid, goods);
                return ResponseUtil.OK("产品映射成功");
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 判断住宿费用是否超过标准
        public ActionResult CheckHotelLimit(string city, int day, float realHotel)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                if (OrgUtil.CheckCEO(employee.EmplID))
                {
                    return ResponseUtil.OK("总经理不需要检测标准");
                }
                int standard = WorkflowUtil.GetHotelStandard(employee.EmplID, city, day * 24);
                if (realHotel > standard)
                {
                    return ResponseUtil.Error("住宿费用超过补贴标准");
                }
                else
                {
                    return ResponseUtil.OK("住宿费用检测通过");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
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
                var customFunc = CommonUtils.ParseCustomFunc(eventConfig.Event);
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
                var result = CommonUtils.caller(customFunc.ClassName, customFunc.MethodName, resultArguments);
                return ResponseUtil.OK(result);
            }
            catch (TargetInvocationException e)
            {
                return ResponseUtil.Error(e.InnerException.Message);
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 判断报销是否能直接填写
        [Authorize]
        public ActionResult CheckDirectRefund()
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                var isCEO = OrgUtil.CheckCEO(employee.EmplID);
                return ResponseUtil.OK(new
                {
                    isCEO = isCEO
                });
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
            try
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
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 判断是否是无补贴人员
        public ActionResult CheckNoAllowance()
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                var noAllowanceUser = OrgUtil.CheckRole(employee.EmplID, "无补贴人员");
                return ResponseUtil.OK(new
                {
                    shouldRemove = noAllowanceUser
                });
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 移除表单中公式
        public ActionResult RemoveFormula(string mid, string user, string role, string formula)
        {
            try
            {
                var shouldRemove = OrgUtil.CheckRole(user, role);
                if (shouldRemove)
                {
                    var message = mgr.GetMessage(mid);
                    Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
                    Worksheet worksheet = doc.Worksheet;
                    var oldDocument = worksheet.Document.Replace(formula, "");
                    worksheet.Document = oldDocument;
                    sheetMgr.UpdateWorksheet(worksheet);
                    return ResponseUtil.OK("移除公式成功");
                }
                else
                {
                    return ResponseUtil.OK("无需移除公式");
                }

            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 判断是否是司机
        [Authorize]
        public ActionResult CheckDriver()
        {
            var employee = (User.Identity as AppkizIdentity).Employee;
            var isDriver = OrgUtil.CheckRole(employee.EmplID, "司机");
            return ResponseUtil.OK(new
            {
                isDriver = isDriver
            });
        }
        #endregion

        #region 判断两个日期是否是同一天
        public ActionResult CheckSameDay(string selectType, string BeginDate, string EndDate)
        {
            try
            {
                if (selectType == DEP_Constants.DATE_SELECT_TYPE_HOUR)
                {
                    var startDate = DateTime.Parse(BeginDate);
                    var endDate = DateTime.Parse(EndDate);
                    if (startDate.Date.ToString("yyyy-MM-dd") != endDate.Date.ToString("yyyy-MM-dd"))
                    {
                        return ResponseUtil.Error("开始日期和结束日期必须为同一天");
                    }
                }
                return ResponseUtil.OK("校验成功");
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion
        
        #region 检查列表明细必填项
        public ActionResult CheckLineRequired(string mid, int startRow, int endRow, string checkCols)
        {
            try
            {
                var cols = checkCols.Split(',').Select(int.Parse).ToList();
                Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
                Worksheet worksheet = doc.Worksheet;
                for (int i = startRow; i <= endRow; i++)
                {
                    var lineCells = new List<Workcell>();
                    foreach (var col in cols)
                    {
                        var workCell = worksheet.GetWorkcell(i, col);
                        lineCells.Add(workCell);
                    }

                    if (!(CellAllEmpty(lineCells) || CellAllFull(lineCells)))
                    {
                        return ResponseUtil.Error("请检查明细必填项");
                    }
                }

                return ResponseUtil.OK("检查通过");
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }

        public bool CellAllEmpty(List<Workcell> cells)
        {
            return cells.Count(n => string.IsNullOrEmpty(n.WorkcellValue)) == cells.Count;
        }

        public bool CellAllFull(List<Workcell> cells)
        {
            return cells.Count(n => string.IsNullOrEmpty(n.WorkcellValue)) == 0;
        }
        #endregion
    }
}