using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using DeptOA.Entity;
using DeptOA.Common;
using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;

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
             * 参数获取
             */
            // 消息ID
            var MessageID = collection["mid"];
            // 节点ID
            var NodeID = collection["nid"];

            
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(MessageID));
            Worksheet worksheet = doc.Worksheet;
            Message message = mgr.GetMessage(MessageID);

            /*
             * 配置读取
             */
            using (StreamReader sr = new StreamReader(Server.MapPath("~/mapping.json")))
            {
                var mappingModel = JsonConvert.DeserializeObject<DEP_MappingModel>(sr.ReadToEnd());
                var mapping = mappingModel.mapping;
                var fields = mapping.fields;

                List<Dictionary<string, string>> values = new List<Dictionary<string, string>>();
                foreach(var field in fields)
                {
                    Dictionary<string, string> dict = new Dictionary<string, string>();

                    //公文标题
                    var FieldValue = worksheet.GetWorkcell(field.row, field.col);
                    dict.Add(field.key, FieldValue == null ? "" : FieldValue.WorkcellValue);

                    values.Add(dict);
                }

                return ResponseUtil.OK(values);
            }
        }
        #endregion
    }
}