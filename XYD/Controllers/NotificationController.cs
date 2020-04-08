using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Models;
using XYD.Common;
using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using System.Threading.Tasks;
using System.Configuration;

namespace XYD.Controllers
{
    public class NotificationController : Controller
    {
        WorkflowMgr mgr = new WorkflowMgr();
        OrgMgr orgMgr = new OrgMgr();

        #region 流程审批结束
        public async Task<ActionResult> MessageUpdateEvent(String ID)
        {
            try
            {
                var message = mgr.GetMessage(ID);
                var startUser = orgMgr.GetEmployee(message.MessageIssuedBy);
                var email = orgMgr.GetEmplContactInfo(startUser.EmplID, "email");
                var subject = string.Format("{0}审批已完成", message.MessageTitle);
                var body = string.Format(@"您的申请审批已完成，
                                           <a href='{0}/Apps/Workflow/Running/Open?mid={1}'>点我查看详情</a>",
                                           ConfigurationManager.AppSettings["ServerDomain"], message.MessageID);
                await Task.Run(() => { new MailHelper().SendAsync(subject, body, email, null); });
                return ResponseUtil.OK("send success");
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion
    }
}