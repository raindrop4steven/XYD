using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using Appkiz.Library.Security.Authentication;
using DeptOA.Common;
using DeptOA.Entity;
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
    }
}