using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Models;

namespace XYD.Controllers
{
    public class BackupMoneyController : Controller
    {
        WorkflowMgr mgr = new WorkflowMgr();
        SheetMgr sheetMgr = new SheetMgr();
        OrgMgr orgMgr = new OrgMgr();

        #region 导入备用金
        public ActionResult Import(string mid, XYD_BackupMoney model)
        {
            try
            {
                // 根据流程ID获得发起人
                var message = mgr.GetMessage(mid);
                var applyUser = orgMgr.GetEmployee(message.MessageIssuedBy);
                model.MessageID = mid;
                model.EmplID = applyUser.EmplID;
                model.EmplName = applyUser.EmplName;
                using (var db = new DefaultConnection())
                {
                    db.BackupMoney.Add(model);
                    db.SaveChanges();
                    return ResponseUtil.OK("备用金添加成功");
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