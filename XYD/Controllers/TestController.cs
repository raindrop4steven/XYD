using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Entity;

namespace XYD.Controllers
{
    public class TestController : Controller
    {
        WorkflowMgr mgr = new WorkflowMgr();
        SheetMgr sheetMgr = new SheetMgr();
        OrgMgr orgMgr = new OrgMgr();

        #region 测试解析发起流程配置
        public ActionResult ParseCellConfig(string MessageID)
        {
            try
            {
                Message message = mgr.GetMessage(MessageID);
                Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(MessageID));
                Worksheet worksheet = doc.Worksheet;

                var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}-start.json", message.FromTemplate));

                using (StreamReader sr = new StreamReader(filePathName))
                {
                    var fields = JsonConvert.DeserializeObject<XYD_Fields>(sr.ReadToEnd(), new XYDCellJsonConverter());

                    foreach (XYD_Base_Cell cell in fields.Fields)
                    {
                        // 查找对应的值
                        WorkflowUtil.FillCellValue(worksheet, cell);
                    }
                    return ResponseUtil.OK(fields);
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 审批
        [HttpPost]
        public ActionResult Audit(FormCollection collection)
        {
            var mid = collection["mid"];
            var nid = collection["nid"];
            var operate = collection["operate"];
            var opinion = collection["opinion"];

            WorkflowUtil.AuditMessage(mid, nid, operate, opinion);
            return ResponseUtil.OK("审批OK");
        }
        #endregion
    }
}