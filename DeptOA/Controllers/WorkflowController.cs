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

        #region 公文详情
        [HttpPost]
        public ActionResult GetDetailInfo(FormCollection collection)
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

            try
            {
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
                    var detail = wkfService.GetDetailInfo(MessageID, details);

                    return ResponseUtil.OK(detail);
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
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

            //公文标题
            var documentTitleName = WorkflowUtil.GetCellValue(worksheet, 6, 4, 0);

            //附件信息
            var attachments = WorkflowUtil.GetCellValue(worksheet, 15, 4, 10);

            return ResponseUtil.OK(new
            {
                documentTitleName = documentTitleName,
                attachments = attachments
            });
        }
        #endregion
    }
}