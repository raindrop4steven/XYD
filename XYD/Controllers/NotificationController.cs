using System.Collections.Generic;
using System.Web.Mvc;
using XYD.Common;
using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;

namespace XYD.Controllers
{
    public class NotificationController : Controller
    {
        WorkflowMgr mgr = new WorkflowMgr();
        OrgMgr orgMgr = new OrgMgr();

        #region 发送提交通知
        public ActionResult SendSubmitNotification()
        {
            /*
                 * 参数获取
                 */
            // 流程ID
            var MessageID = Request.Params["MessageID"];
            // 用户ID
            var UserID = Request.Params["UserID"];
            // 节点ID
            var NodeKey = Request.Params["NodeKey"];

            var message = mgr.GetMessage(MessageID);

            // 发送极光推送消息
            Dictionary<string, object> dict = new Dictionary<string, object>();

            dict["type"] = DEP_Constants.JPush_Workflow_Type;
            dict["title"] = string.Format("您有新的流程提醒通知 \"{0}\"", message.MessageTitle);
            dict["content"] = new Dictionary<string, object>()
            {
                {"mid",  MessageID}
            };
            WorkflowUtil.SendJPushNotification(new List<string>() { UserID }, dict);

            return ResponseUtil.OK("发送成功");
        }
        #endregion
    }
}