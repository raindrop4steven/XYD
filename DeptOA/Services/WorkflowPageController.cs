using DeptOA.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DeptOA.Common;

namespace DeptOA.Services
{
    public class WorkflowPageController : Controller
    {
        [HttpPost]
        public ActionResult GetPendingInfo(QueryInfo query)
        {
            //当前人
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;

            var sql = new StringBuilder();
            sql.Append(string.Format(@"SELECT ROW_NUMBER () OVER (ORDER BY c.CreateTime DESC) number, b.DocumentNumber,
                                    b.DocumentTitle,
                                    CONVERT(varchar(100), b.ClosedOrHairTime, 20) as ClosedOrHairTime,
                                    b.MessageId,
                                    b.WorkFlowId,
                                    b.SequenceName,
                                    b.SequenceNumber,
                                    --流程发起人
                                    a.MessageIssuedBy AS InitiateEmplId,
                                    d.EmplName AS InitiateEmplName,
                                    --流程类型
                                    a.MessageTitle,
                                    --环节
                                    c.NodeName AS MyTask,
                                    CONVERT(varchar(100), c.CreateTime, 20) as ReceiveTime
                                     FROM WKF_Message a
                                    INNER JOIN GW_ReceiveFile b
                                    ON a.MessageID = b.MessageId
                                    INNER JOIN WKF_MessageHandle c
                                    ON a.MessageID = c.MessageID
                                    INNER JOIN ORG_Employee d
                                    ON a.MessageIssuedBy = d.EmplID
                                    WHERE c.HandleStatus != 0
                                    and b.IsFirstLoading = 0
                                    and a.MessageStatus != 3 
                                    and (c.UserID = '{0}' or (c.EntrustBy = '{0}' and c.EntrustBy <> ''))", emplId));
            if (query.PageNumber <= 0)
            {
                query.PageNumber = 1;
            }
            if (query.PageSize <= 0)
            {
                query.PageSize = 20;
            }
            if (!string.IsNullOrWhiteSpace(query.SortDirection))
            {
                query.SortDirection = "desc";
            }
            if (!string.IsNullOrWhiteSpace(query.SortColumn))
            {
                query.SortColumn = "ReceiveTime";
            }
            //公文标题
            if (!string.IsNullOrWhiteSpace(query.Title))
            {
                sql.Append(string.Format(@" and b.DocumentTitle like '%{0}%'", query.Title));
            }
            //文号
            if (!string.IsNullOrWhiteSpace(query.DocumentNumber))
            {
                sql.Append(string.Format(@" and b.DocumentNumber like '%{0}%'", query.DocumentNumber));
            }
            // 来文单位
            if (!string.IsNullOrWhiteSpace(query.DocumentUnit))
            {
                sql.Append(string.Format(@" and b.ReceivedDocumentUnit like '%{0}%'", query.DocumentUnit));
            }
            //开始发文时间
            if (query.StartClosedOrHairTime.HasValue)
            {
                sql.Append(string.Format(@" and b.ClosedOrHairTime >= '{0}'", query.StartClosedOrHairTime));
            }
            //结束发文时间
            if (query.EndClosedOrHairTime.HasValue)
            {
                sql.Append(string.Format(@" and b.ClosedOrHairTime <= '{0}'", query.EndClosedOrHairTime));
            }
            //开始接受时间
            if (query.StartReceiveTime.HasValue)
            {
                sql.Append(string.Format(@" and c.CreateTime >= '{0}'", query.StartReceiveTime));
            }
            //结束接收时间
            if (query.EndReceiveTime.HasValue)
            {
                sql.Append(string.Format(@" and c.CreateTime <= '{0}'", query.EndReceiveTime));
            }
            //流程类型
            if (!string.IsNullOrWhiteSpace(query.WorkFlowId))
            {
                sql.Append(string.Format(@" and b.WorkFlowId = '{0}'", query.WorkFlowId));
            }
            //序列名称
            if (!string.IsNullOrWhiteSpace(query.SequenceName))
            {
                sql.Append(string.Format(@" and b.SequenceName = '{0}'", query.SequenceName));
            }
            //序列号
            if (!string.IsNullOrWhiteSpace(query.SequenceNumber))
            {
                sql.Append(string.Format(@" and b.SequenceNumber like '%{0}%'", query.SequenceNumber));
            }
            //模糊检索
            if (!string.IsNullOrWhiteSpace(query.QueryCondition))
            {
                sql.Append(string.Format(@" and (b.DocumentNumbe like '%{0}%' or b.DocumentTitle like '%{0}%')", query.QueryCondition));
            }


            var result = DbUtil.ExecuteSqlCommand(sql.ToString(), DbUtil.GetPendingResult);
            return Json(result);
        }
    }
}