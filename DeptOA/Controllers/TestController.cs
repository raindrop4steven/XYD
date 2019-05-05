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
            return ResponseUtil.OK(WorkflowUtil.GetWorkflowByUser(uid));
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
    }
}