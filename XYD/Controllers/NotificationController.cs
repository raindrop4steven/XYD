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
using XYD.Entity;

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

        #region 文章发布通知
        public async Task<ActionResult> SendCMSEmail(XYD_CMS_Notification notification)
        {
            try
            {
                var findSql = @"select * from ORG_Employee ";
                if (notification.unsavedReaders != null && notification.unsavedReaders.Count > 0)
                {
                    var inSql = string.Join(",", notification.unsavedReaders.Select(n => "'" + n.Trim() + "'"));
                    findSql = findSql + string.Format(" where EmplID in ({0})", inSql);
                }
                
                List<Employee> employees = orgMgr.FindEmployeeBySQL(findSql);
                List<string> emailList = new List<string>();
                foreach(var employee in employees)
                {
                    var email = orgMgr.GetEmplContactInfo(employee.EmplID, "email");
                    if (!string.IsNullOrEmpty(email))
                    {
                        emailList.Add(email);
                    }
                }
                var tos = string.Join(",", emailList);
                var subject = notification.title;
                var url = string.Format(@"{0}{1}", ConfigurationManager.AppSettings["ServerDomain"], notification.url);
                var body = string.Format(@"您有新的公告通知，点击查看详情：<a href='{0}'>{1}</a>", url, notification.title);
                await Task.Run(() => { new MailHelper().SendAsync(subject, body, tos, null); });
                return ResponseUtil.OK("send success");
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 发送提交通知
        public ActionResult SendSubmitNotification()
        {
            try
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
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion
    }
}