using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using Appkiz.Library.Security.Authentication;
using DeptOA.Common;
using DeptOA.Entity;
using DeptOA.Models;
using DeptOA.Services;
using JUST;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace DeptOA.Controllers
{
    public class TestController : Controller
    {
        WorkflowMgr mgr = new WorkflowMgr();
        OrgMgr orgMgr = new OrgMgr();

        #region 测试获得某个人的工作流
        public ActionResult GetWorkflowByUser(string uid)
        {
            return ResponseUtil.OK(WorkflowUtil.GetTablesByUser(uid));
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

        #region 测试Transform结果
        [HttpPost]
        public ActionResult TransDetail(FormCollection collection)
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
                    var detail = wkfService.GetDetailInfo(MessageID, NodeID, details);

                    var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", "transformer"));

                    using (StreamReader sr = new StreamReader(filePathName))
                    {
                        var transformer = sr.ReadToEnd();
                        var stringDetail = Newtonsoft.Json.JsonConvert.SerializeObject(detail);
                        string transformedString = JsonTransformer.Transform(transformer, stringDetail);

                        JObject result = JObject.Parse(transformedString);
                        return ResponseUtil.OK(new
                        {
                            detail = result
                        });
                    }

                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 测试获得某个人的子流程
        public ActionResult GetSubflowByUser(string uid)
        {
            var subflowList = WorkflowUtil.GetSubflowByUser(uid);

            return ResponseUtil.OK(subflowList);
        }
        #endregion

        #region 测试自定义方法
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

        #region 测试通用通知方法
        /// <summary>
        /// 目前针对的是已经处理过公文的用户通知
        /// </summary>
        /// <param name="mid"></param>
        /// <param name="nid"></param>
        /// <returns></returns>
        public ActionResult GeneralNotify(string mid, string nid, string uid)
        {
            /*
             * 变量定义
             */
            // 当前用户
            //var employee = (User.Identity as AppkizIdentity).Employee;
            // SQL语句
            var sql = string.Empty;

            /*
             * 读取配置
             */
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", "notify"));
            using (StreamReader sr = new StreamReader(filePathName))
            {
                List<Receiver> receivers = null;

                var notifies = JsonConvert.DeserializeObject<DEP_Notifies>(sr.ReadToEnd());

                foreach(var notify in notifies.notify)
                {
                    if(notify.originNode == nid)
                    {
                        receivers = notify.receivers;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                // 判断是否有通知节点
                if (receivers == null)
                {
                    return ResponseUtil.Error(string.Format("消息{0} 节点{1} 无通知配置", mid, nid));
                }
                else
                {
                    // 如果配置为空，则默认所有人员都会收到通知
                    if(receivers.Count == 0)
                    {
                        // 所有其他人
                        sql = string.Format(@"select 
                                                        DISTINCT HandledBy
                                                    from 
	                                                    WKF_WorkflowHistory a
                                                    inner join
	                                                    ORG_Employee b
                                                    on
	                                                    a.MessageID='{0}'
                                                    and
	                                                    a.HandlerDeptID <> ''
                                                    AND
	                                                    a.HandledBy = b.EmplID
                                                    AND
                                                        a.HandledBy <> '{1}'", mid, uid);
                    }
                    else
                    {
                        var statements = new List<StringBuilder>();

                        foreach(var receiver in receivers)
                        {
                            string nodeKey = receiver.nid;
                            int scope = receiver.scope;

                            StringBuilder sb = new StringBuilder();

                            sb.Append(string.Format(@"select 
                                                          DISTINCT HandledBy
                                                        from 
	                                                        WKF_WorkflowHistory a
                                                        inner join
	                                                        ORG_Employee b
                                                        on
	                                                        a.MessageID='{0}'
                                                        and
	                                                        a.HandlerDeptID <> ''
                                                        AND
	                                                        a.HandledBy = b.EmplID
                                                        AND
                                                          a.HandledBy <> '{1}'
                                                        and
	                                                        a.NodeKey = '{2}'", mid, uid, nodeKey));
                            // 判断是否有等级
                            if (scope == 1) // 等级高
                            {
                                sb.Append(string.Format(@" and b.GlobalSortNo > (select GlobalSortNo from ORG_Employee WHERE EmplID='{0}')", uid));
                            }
                            else if (scope == 2)
                            {
                                sb.Append(string.Format(@" and b.GlobalSortNo < (select GlobalSortNo from ORG_Employee WHERE EmplID='{0}')", uid));
                            }
                            else
                            {
                                // 无等级或不支持的等级
                            }

                            statements.Add(sb);
                        }

                        sql = string.Join(" UNION ", statements);
                    }

                    var receiverList = DbUtil.ExecuteSqlCommand(sql, DbUtil.GetNotifyReceivers);

                    return ResponseUtil.OK(receiverList);
                }
            }
        }
        #endregion

        #region 获得Cell中意见
        public ActionResult GeneralOpinion(string mid)
        {
            /*
             * 变量定义
             */
            // 修改意见列表
            var opinionList = new List<object>();
            // 是否可以修改意见
            var canChaneOpinion = true;
            // TODO: 多人审批的排序规则，放到每个意见里面
            var Order = "ASC";
            // 当前用户
            var employee = (User.Identity as AppkizIdentity).Employee;

            /*
             * 获取流程表单
             */
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
            Worksheet worksheet = doc.Worksheet;

            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", "opinion"));
            using (StreamReader sr = new StreamReader(filePathName))
            {
                Opinion resultOpinion = null;
                var opinions = JsonConvert.DeserializeObject<DEP_Opinions>(sr.ReadToEnd());
                foreach(var opinion in opinions.opinions)
                {
                    var history = string.Empty;
                    var editIcon = string.Empty;

                    List<WorkflowHistory> workflowHistory = mgr.FindWorkflowHistory(mid, opinion.node);

                    if (workflowHistory.Count == 0)
                    {
                        //节点没有未处理
                        continue;
                    }
                    else
                    {
                        // 判断这个节点
                        foreach (var wkhistory in workflowHistory)
                        {
                            if (wkhistory.HandledBy == employee.EmplID)
                            {
                                editIcon = "fa fa-edit";
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (opinion.type == 0)
                        {
                            // 普通意见
                            var workcell = worksheet.GetWorkcell(resultOpinion.value.row, resultOpinion.value.col);
                            var cellValue = workcell == null ? string.Empty : workcell.WorkcellValue;

                            using (var db = new DefaultConnection())
                            {
                                var nibanOpinion = db.Opinion.Where(n => n.MessageID == mid && n.NodeKey == opinion.node && n.EmplID == employee.EmplID).OrderByDescending(n => n.order).FirstOrDefault();
                                // 获得最新的意见
                                if (nibanOpinion != null)
                                {
                                    history = "<div class=\"my-niban-opinion\"><span style =\"word-wrap: break-word;\">" + nibanOpinion.Opinion + "</span><i class=\"" + editIcon + "\" style=\"margin-left:8px\"></i></div>";
                                }
                                else
                                {
                                    history = "<div class=\"my-niban-opinion\"><span style =\"word-wrap: break-word;\">" + cellValue + "</span><i class=\"" + editIcon + "\" style=\"margin-left:8px\"></i></div>";
                                }
                            }
                        }
                        else if (opinion.type == 1)
                        {
                            // 多人意见
                            // 排序(默认从1开始)
                            var nodeOrderDict = new Dictionary<string, int>();
                            // 每个用户对应签批数量
                            var nodeDict = new Dictionary<string, int>();
                            List<KeyValuePair<string, string>> itemList = new List<KeyValuePair<string, string>>();
                            List<string> list = new List<string>();

                            using (var db = new DefaultConnection())
                            {
                                // 新意见
                                var newOpinionList = db.Opinion.Where(n => n.MessageID == mid && n.NodeKey == opinion.node).OrderBy(n => n.order).ToList();
                                // 原始意见，并替换对应新意见
                                var workcellValue = worksheet.GetWorkcell(resultOpinion.value.row, resultOpinion.value.col);
                                XmlNodeList deptHistory = workcellValue.History;

                                foreach (XmlNode node in deptHistory)
                                {
                                    var emplId = node.Attributes.GetNamedItem("emplId").InnerText;
                                    // 更新排序
                                    if (nodeDict.ContainsKey(emplId))
                                    {
                                        int order = nodeDict[emplId];
                                        nodeDict[emplId] = order + 1;
                                    }
                                    else
                                    {
                                        nodeDict.Add(emplId, 1);
                                    }
                                }

                                foreach (XmlNode node in deptHistory)
                                {
                                    var emplId = node.Attributes.GetNamedItem("emplId").InnerText;
                                    var value = node.Attributes.GetNamedItem("Value").InnerText;
                                    var updateTime = node.Attributes.GetNamedItem("UpdateTime").InnerText;

                                    // 更新排序
                                    if (nodeOrderDict.ContainsKey(emplId))
                                    {
                                        int order = nodeOrderDict[emplId];
                                        nodeOrderDict[emplId] = order + 1;
                                    }
                                    else
                                    {
                                        nodeOrderDict.Add(emplId, 1);
                                    }

                                    var currentClass = "";
                                    var orderString = string.Empty;
                                    // 查找是否有对应的新意见，使用新意见替换旧意见
                                    foreach (DEP_Opinion newOpinion in newOpinionList)
                                    {
                                        // 同一个人的意见，且顺序相同就替换对应的签批意见
                                        if (newOpinion.EmplID == emplId && nodeOrderDict[emplId] == newOpinion.order)
                                        {
                                            value = newOpinion.Opinion;
                                            updateTime = newOpinion.UpdatedTime.Value.ToString("yyyy-%M-dd HH:mm");
                                            orderString = Convert.ToString(newOpinion.order);
                                            break;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    // 如果是当前用户，且是最后一条记录，则设置可修改
                                    // 流程未结束，可以修改的情况下才可以修改
                                    if (emplId == employee.EmplID && nodeDict[emplId] == nodeOrderDict[emplId])
                                    {
                                        currentClass = "my-opinion";
                                        editIcon = "fa fa-edit";
                                    }

                                    itemList.Add(new KeyValuePair<string, string>(emplId, "<div class=\"history - item " + currentClass + "\" order=\"" + orderString + "\"><div class=\"history-people\">{0}(" + updateTime + "):</div><span style=\"word-wrap: break-word;\">" + value + "</span><i class=\"" + editIcon + "\" style=\"margin-left:8px\"></i></div>"));
                                    list.Add(emplId);
                                }
                                if (list.Count > 0)
                                {
                                    var sql = @"SELECT EmplID, EmplName FROM ORG_Employee WHERE EmplID IN ({0}) ORDER BY GlobalSortNo " + Order;
                                    var results = DbUtil.ExecuteSqlCommand(string.Format(sql, string.Join(",", list)), DbUtil.WKF_GlobalSortNo);
                                    Dictionary<string, string> orderredDict = (Dictionary<string, string>)results.ElementAt(0);
                                    StringBuilder sb = new StringBuilder();

                                    foreach (KeyValuePair<string, string> item in orderredDict)
                                    {
                                        foreach (KeyValuePair<string, string> innerItem in itemList)
                                        {
                                            if (innerItem.Key == item.Key)
                                            {
                                                sb.Append(string.Format(innerItem.Value, item.Value));
                                            }
                                        }
                                    }

                                    history = sb.ToString();
                                    opinionList.Add(history);
                                }
                                else
                                {
                                    // 已有处理记录而xml中并无意见，理论上不会出现
                                    history = string.Empty;
                                }
                            }
                        }
                        else
                        {
                            return ResponseUtil.Error(string.Format("不支持的意见类型{0}", resultOpinion.type));
                        }
                        // 将节点与签批记录统一返回到前端
                        opinionList.Add(new
                        {
                            node = opinion.node,
                            history = history
                        });
                    }
                }
                
                return ResponseUtil.OK(opinionList);
            }
        }
        #endregion

        #region 根据流程mid获得对应的Nodekey
        [HttpPost]
        public ActionResult GetHandledNodeKey(FormCollection collection)
        {
            try
            {
                /*
                 * 变量定义
                 */
                // 获得当前用户
                var employee = (User.Identity as AppkizIdentity).Employee;
                // 工作流Manager
                var workflowMgr = new WorkflowMgr();
                // 当前人处理节点列表
                var nodeKeyList = new List<string>();

                /*
                 * 参数获取
                 */
                // 流程ID
                var mid = collection["mid"];

                /*
                 * 参数校验
                 */
                // 流程ID
                if (string.IsNullOrEmpty(mid))
                {
                    return new JsonNetResult(new
                    {
                        Succeed = false,
                        Message = "流程ID不能为空"
                    });
                }

                /*
                 * 获取对应的流程处理记录
                 */
                Message message = workflowMgr.GetMessage(mid);
                if (message == null)
                {
                    return new JsonNetResult(new
                    {
                        Succeed = false,
                        Message = "流程不存在"
                    });
                }
                else
                {
                    // 获得对应的工作流历史
                    List<WorkflowHistory> workflowHistoryList = message.History;
                    foreach (WorkflowHistory history in workflowHistoryList)
                    {
                        // 判断是否是当前用户的节点
                        if (history.HandledBy == employee.EmplID)
                        {
                            nodeKeyList.Add(history.NodeKey);
                        }
                    }
                    return new JsonNetResult(new
                    {
                        Succeed = true,
                        Data = new
                        {
                            nodekeys = nodeKeyList
                        }
                    });
                }
            }
            catch
            {
                return new JsonNetResult(new
                {
                    Succeed = false,
                    Message = "用户未登录"
                });
            }
        }
        #endregion

        #region 添加意见修改记录
        /// <summary>
        /// url: /Apps/Tiger/Workflow/AddNewOpinion
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddNewOpinion(FormCollection collection)
        {
            try
            {
                /*
                 * 变量定义
                 */
                // 当前用户
                var employee = (User.Identity as AppkizIdentity).Employee;
                // 流程管理
                WorkflowMgr mgr = new WorkflowMgr();
                // 当前用户评论总数
                int userTotalClount = 0;
                // 顺序
                int order = 0;

                /*
                 * 参数获取
                 */
                // 流程ID
                var mid = collection["mid"];
                // 节点ID
                var nid = collection["nid"];
                // 意见
                var opinion = collection["opinion"];
                // 顺序
                var orderString = collection["order"];

                /*
                 * 参数校验
                 */
                // 流程ID
                if (string.IsNullOrEmpty(mid))
                {
                    return new JsonNetResult(new
                    {
                        Succeed = false,
                        Message = "消息ID不能为空"
                    });
                }
                // 节点ID
                if (string.IsNullOrEmpty(nid))
                {
                    return new JsonNetResult(new
                    {
                        Succeed = false,
                        Message = "节点ID不能为空"
                    });
                }
                // 意见
                if (string.IsNullOrEmpty(opinion))
                {
                    return new JsonNetResult(new
                    {
                        Succeed = false,
                        Message = "意见不能为空"
                    });
                }
                // 排序
                if (!string.IsNullOrEmpty(orderString))
                {
                    if (!int.TryParse(orderString, out order))
                    {
                        return new JsonNetResult(new
                        {
                            Succeed = false,
                            Message = "排序应为数字"
                        });
                    }
                }
                /*
                 * 插入意见
                 */
                Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
                Worksheet worksheet = doc.Worksheet;
                Workcell workcell = worksheet.GetWorkcell(10, 4);
                XmlNodeList history = workcell.History;
                foreach (XmlNode node in history)
                {
                    var emplId = node.Attributes.GetNamedItem("emplId").InnerText;
                    // 统计总数
                    if (employee.EmplID == emplId)
                    {
                        userTotalClount = userTotalClount + 1;
                    }
                }
                // 排序字段为空，则插入新的记录，新纪录的排序字段和总数保持一致（最新）
                // 由于是修改，且无法删除，所以userTotalCount总是会大于0
                var db = new DefaultConnection();

                if (string.IsNullOrEmpty(orderString))
                {
                    DEP_Opinion insertOpinion = new DEP_Opinion();
                    insertOpinion.EmplID = employee.EmplID;
                    insertOpinion.MessageID = mid;
                    insertOpinion.NodeKey = nid;
                    insertOpinion.Opinion = opinion;
                    insertOpinion.order = userTotalClount;
                    insertOpinion.CreateTime = DateTime.Now;
                    insertOpinion.UpdatedTime = DateTime.Now;
                    db.Opinion.Add(insertOpinion);
                }
                else
                {
                    // 更新已有记录
                    var newOpinion = db.Opinion.Where(n => n.MessageID == mid && n.EmplID == employee.EmplID && n.order == order).FirstOrDefault();
                    if (newOpinion == null)
                    {
                        return new JsonNetResult(new
                        {
                            Succeed = false,
                            Message = "更新的记录不存在"
                        });
                    }
                    else
                    {
                        newOpinion.Opinion = opinion;
                        newOpinion.UpdatedTime = DateTime.Now;
                    }
                }

                db.SaveChanges();

                /*
                 * 修改后发送通知
                 */
                //SendChangeNotify(collection);
                //// 意见不为【已阅】时，发送给文电科通知
                //if (opinion.Trim() != WHConstants.Workflow_Opinion_Read)
                //{
                //    SendLeaderNotify(mid, nid, employee);
                //}

                return new JsonNetResult(new
                {
                    Succeed = true,
                    Message = "修改成功"
                });
            }
            catch
            {
                return new JsonNetResult(new
                {
                    Succeed = false,
                    Message = "添加意见失败"
                });
            }
        }
        #endregion
    }
}