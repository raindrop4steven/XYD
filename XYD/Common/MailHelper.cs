using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace XYD.Common
{
    public class MailHelper
    {
        public delegate int MethodDelegate(int x, int y);
        private readonly int smtpPort = int.Parse(ConfigurationManager.AppSettings["MailServerPort"]);
        readonly string SmtpServer = ConfigurationManager.AppSettings["MailServer"];
        private readonly string UserName = ConfigurationManager.AppSettings["MailServerUserName"];
        readonly string Pwd = ConfigurationManager.AppSettings["MailServerUserPWD"];
        private readonly string AuthorName = "OA 邮件提醒";
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Tos { get; set; }
        public bool EnableSsl { get; set; }
        MailMessage GetClient
        {
            get
            {
                if (string.IsNullOrEmpty(Tos)) return null;
                MailMessage mailMessage = new MailMessage();
                //多个接收者                
                foreach (string _str in Tos.Split(','))
                {
                    mailMessage.To.Add(_str);
                }
                mailMessage.From = new System.Net.Mail.MailAddress(UserName, AuthorName);
                mailMessage.Subject = Subject;
                mailMessage.Body = Body;
                mailMessage.IsBodyHtml = true;
                mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;
                mailMessage.Priority = System.Net.Mail.MailPriority.High;
                return mailMessage;
            }
        }
        SmtpClient GetSmtpClient
        {
            get
            {
                return new SmtpClient
                {
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(UserName, Pwd),
                    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                    Host = SmtpServer,
                    Port = smtpPort,
                    EnableSsl = EnableSsl,
                };
            }
        }
        //回调方法
        Action<string> actionSendCompletedCallback = null;
        ///// <summary>
        ///// 使用异步发送邮件
        ///// </summary>
        ///// <param name="subject">主题</param>
        ///// <param name="body">内容</param>
        ///// <param name="to">接收者,以,分隔多个接收者</param>
        //// <param name="_actinCompletedCallback">邮件发送后的回调方法</param>
        ///// <returns></returns>
        public void SendAsync(string subject, string body, string to, Action<string> _actinCompletedCallback)
        {
            if (string.IsNullOrEmpty(to)) return;

            Tos = to;

            SmtpClient smtpClient = GetSmtpClient;
            MailMessage mailMessage = GetClient;
            if (smtpClient == null || mailMessage == null) return;
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;
            EnableSsl = false;
            //发送邮件回调方法
            actionSendCompletedCallback = _actinCompletedCallback;
            smtpClient.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
            try
            {
                smtpClient.SendAsync(mailMessage, "true");//异步发送邮件,如果回调方法中参数不为"true"则表示发送失败
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                smtpClient = null;
                mailMessage = null;
            }
        }
        /// <summary>
        /// 异步操作完成后执行回调方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendCompletedCallback(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //同一组件下不需要回调方法,直接在此写入日志即可
            //写入日志
            //return;
            if (actionSendCompletedCallback == null) return;
            string message = string.Empty;
            if (e.Cancelled)
            {
                message = "异步操作取消";
            }
            else if (e.Error != null)
            {
                message = (string.Format("UserState:{0},Message:{1}", (string)e.UserState, e.Error.ToString()));
            }
            else
                message = (string)e.UserState;
            //执行回调方法
            actionSendCompletedCallback(message);
        }
    }
}