using DeptOA.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DeptOA.Common;

namespace DeptOA.Controllers
{
    public class WorkflowPageController : Controller
    {
        #region 待处理公文
        [HttpPost]
        public ActionResult GetPendingInfo(QueryInfo query)
        {
            //当前人
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;

            var sql = new StringBuilder();
            sql.Append(string.Format(@"SELECT ROW_NUMBER () OVER (ORDER BY c.CreateTime DESC) number,
                                    b.DocumentTitle,
                                    CONVERT(varchar(100), b.ClosedOrHairTime, 20) as ClosedOrHairTime,
                                    b.MessageId,
                                    b.WorkFlowId,
                                    --流程发起人
                                    a.MessageIssuedBy AS InitiateEmplId,
                                    d.EmplName AS InitiateEmplName,
                                    --流程类型
                                    a.MessageTitle,
                                    --环节
                                    c.NodeName AS MyTask,
                                    CONVERT(varchar(100), c.CreateTime, 20) as ReceiveTime
                                     FROM WKF_Message a
                                    INNER JOIN DEP_ReceiveFile_ZhuJian b
                                    ON a.MessageID = b.MessageId
                                    INNER JOIN WKF_MessageHandle c
                                    ON a.MessageID = c.MessageID
                                    INNER JOIN ORG_Employee d
                                    ON a.MessageIssuedBy = d.EmplID
                                    WHERE c.HandleStatus != 0
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

            //开始位置
            var startPage = query.PageSize * (query.PageNumber - 1) + 1;
            //结束位置
            var endPage = startPage + query.PageSize;

            int totalCount = DbUtil.ExecuteScalar(string.Format(@"select count(0) from ({0}) as a", sql));
            var sqlPage = string.Format(@"select a.* from ({0}) a where a.number >= {1} and a.number < {2}", sql, startPage, endPage);

            var result = DbUtil.ExecuteSqlCommand(sqlPage, DbUtil.GetPendingResult);
            return ResponseUtil.OK(new
            {
                totalCount = totalCount,
                data = result
            });
        }
        #endregion

        #region 已处理过公文
        [HttpPost]
        public ActionResult GetDealWithInfo(QueryInfo query)
        {
            //当前人
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;

            var sql = new StringBuilder();
            sql.Append(string.Format(@"SELECT ROW_NUMBER () OVER (ORDER BY d.HandledTime DESC) number,
                                    b.DocumentTitle,
                                    CONVERT(varchar(100), b.ClosedOrHairTime, 20) as ClosedOrHairTime,
                                    b.MessageId,
                                    b.WorkFlowId,
                                    --流程类型
                                    a.MessageTitle,
                                    CONVERT(varchar(100), a.MessageCreateTime, 20) AS CreateTime,
                                    CONVERT(varchar(100), d.HandledTime, 20) AS ReceiveTime
                                     FROM WKF_Message a
                                    INNER JOIN DEP_ReceiveFile_ZhuJian b
                                    ON a.MessageID = b.MessageId
                                    INNER JOIN (select MAX(HandledTime) as HandledTime,MessageID,HandledBy from WKF_WorkflowHistory 
									where NodeKey != 'NODE0001' group by MessageID,HandledBy) as d
                                    ON a.MessageID = d.MessageID
                                    WHERE d.HandledBy = '{0}'  
                                    and a.MessageStatus != 3", emplId));
            if (query.PageNumber <= 0)
            {
                query.PageNumber = 1;
            }
            if (query.PageSize <= 0)
            {
                query.PageSize = 20;
            }
            //公文标题
            if (!string.IsNullOrWhiteSpace(query.Title))
            {
                sql.Append(string.Format(@" and b.DocumentTitle like '%{0}%'", query.Title));
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
            //开始发起时间
            if (query.StartCreatedTime.HasValue)
            {
                sql.Append(string.Format(@" and a.MessageCreateTime >= '{0}'", query.StartCreatedTime));
            }
            //结束发起时间
            if (query.EndCreatedTime.HasValue)
            {
                sql.Append(string.Format(@" and a.MessageCreateTime <= '{0}'", query.EndCreatedTime));
            }
            //开始接收时间
            if (query.StartReceiveTime.HasValue)
            {
                sql.Append(string.Format(@" and d.HandledTime <= {0}", query.StartReceiveTime));
            }
            //结束接收时间
            if (query.EndReceiveTime.HasValue)
            {
                sql.Append(string.Format(@" and d.HandledTime >= {0}", query.EndReceiveTime));
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
            //开始位置
            var startPage = query.PageSize * (query.PageNumber - 1) + 1;
            //结束位置
            var endPage = startPage + query.PageSize;

            int totalCount = DbUtil.ExecuteScalar(string.Format(@"select count(0) from ({0}) as a", sql));
            var sqlPage = string.Format(@"select a.* from ({0}) a where a.number >= {1} and a.number < {2}", sql, startPage, endPage);

            var result = DbUtil.ExecuteSqlCommand(sqlPage, DbUtil.GetDealResult);
            return ResponseUtil.OK(new
            {
                totalCount = totalCount,
                data = result
            });
        }
        #endregion

        #region 我发出未完成
        [HttpPost]
        public ActionResult GetNoCompleteInfo(QueryInfo query)
        {
            //当前人
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;

            var sql = new StringBuilder();
            sql.Append(string.Format(@"SELECT ROW_NUMBER () OVER (ORDER BY a.MessageCreateTime DESC) number,
                                     b.DocumentTitle,
                                     CONVERT(varchar(100), b.ClosedOrHairTime, 20) as ClosedOrHairTime,
                                     b.MessageId,
                                     b.WorkFlowId,
                                     --流程类型
                                     a.MessageTitle,
                                     --环节
                                     c.NodeName AS MyTask,
                                     CONVERT(varchar(100), a.MessageCreateTime, 20) AS CreateTime
                                     FROM WKF_Message a
                                    INNER JOIN DEP_ReceiveFile_ZhuJian b
                                    ON a.MessageID = b.MessageId
                                    INNER JOIN WKF_MessageHandle c
                                    ON a.MessageID = c.MessageID
                                    WHERE a.MessageStatus != 2
                                    AND c.HandleStatus != 0
                                    and a.MessageStatus != 3
                                    and a.MessageIssuedBy = '{0}'", emplId));
            var groupInfo = string.Format(@" GROUP BY a.MessageID, a.MessageCreateTime,
                                     b.DocumentTitle,
                                     b.ClosedOrHairTime,b.MessageId,
                                     b.WorkFlowId,
									--流程类型
                                     a.MessageTitle,
                                     --环节
                                     c.NodeName,
                                     a.MessageCreateTime ");
            if (query.PageNumber <= 0)
            {
                query.PageNumber = 1;
            }
            if (query.PageSize <= 0)
            {
                query.PageSize = 20;
            }
            //公文标题
            if (!string.IsNullOrWhiteSpace(query.Title))
            {
                sql.Append(string.Format(@" and b.DocumentTitle like '%{0}%'", query.Title));
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
            //开始发起时间
            if (query.StartCreatedTime.HasValue)
            {
                sql.Append(string.Format(@" and a.MessageCreateTime >= '{0}'", query.StartCreatedTime));
            }
            //结束发起时间
            if (query.EndCreatedTime.HasValue)
            {
                sql.Append(string.Format(@" and a.MessageCreateTime <= '{0}'", query.EndCreatedTime));
            }
            //流程类型
            if (!string.IsNullOrWhiteSpace(query.WorkFlowId))
            {
                sql.Append(string.Format(@" and b.WorkFlowId = '{0}'", query.WorkFlowId));
            }

            sql.Append(groupInfo);

            //开始位置
            var startPage = query.PageSize * (query.PageNumber - 1) + 1;
            //结束位置
            var endPage = startPage + query.PageSize;

            int totalCount = DbUtil.ExecuteScalar(string.Format(@"select count(0) from ({0}) as a", sql));
            var sqlPage = string.Format(@"select a.* from ({0}) a where a.number >= {1} and a.number < {2}", sql, startPage, endPage);

            var result = DbUtil.ExecuteSqlCommand(sqlPage, DbUtil.GetNoCompleteResult);
            return ResponseUtil.OK(new
            {
                totalCount = totalCount,
                data = result
            });
        }
        #endregion

        #region 我发出已完成
        [HttpPost]
        public ActionResult GetCompleteInfo(QueryInfo query)
        {
            //当前人
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;

            var sql = new StringBuilder();
            sql.Append(string.Format(@"SELECT ROW_NUMBER () OVER (ORDER BY a.MessageCreateTime DESC) number,
                                     b.DocumentTitle,
                                     CONVERT(varchar(100), b.ClosedOrHairTime, 20) AS ClosedOrHairTime,
                                     b.MessageId,
                                     b.WorkFlowId,
                                     --流程类型
                                     a.MessageTitle,
                                     CONVERT(varchar(100), a.MessageCreateTime, 20) AS CreateTime,
                                     CONVERT(varchar(100), a.MessageFinishTime, 20) AS FinishTime
                                     FROM WKF_Message a
                                    INNER JOIN DEP_ReceiveFile_ZhuJian b
                                    ON a.MessageID = b.MessageId
                                    WHERE a.MessageStatus = 2
                                    and a.MessageIssuedBy = '{0}'", emplId));
            if (query.PageNumber <= 0)
            {
                query.PageNumber = 1;
            }
            if (query.PageSize <= 0)
            {
                query.PageSize = 20;
            }
            //公文标题
            if (!string.IsNullOrWhiteSpace(query.Title))
            {
                sql.Append(string.Format(@" and b.DocumentTitle like '%{0}%'", query.Title));
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
            //开始发起时间
            if (query.StartCreatedTime.HasValue)
            {
                sql.Append(string.Format(@" and a.MessageCreateTime >= '{0}'", query.StartCreatedTime));
            }
            //结束发起时间
            if (query.EndCreatedTime.HasValue)
            {
                sql.Append(string.Format(@" and a.MessageCreateTime <= '{0}'", query.EndCreatedTime));
            }
            //开始完成时间
            if (query.StartEndTime.HasValue)
            {
                sql.Append(string.Format(@" and a.MessageFinishTime >= '{0}'", query.StartEndTime));
            }
            //结束完成时间
            if (query.EndEndTime.HasValue)
            {
                sql.Append(string.Format(@" and a.MessageFinishTime <= '{0}'", query.EndEndTime));
            }
            //流程类型
            if (!string.IsNullOrWhiteSpace(query.WorkFlowId))
            {
                sql.Append(string.Format(@" and b.WorkFlowId = '{0}'", query.WorkFlowId));
            }
            //开始位置
            var startPage = query.PageSize * (query.PageNumber - 1) + 1;
            //结束位置
            var endPage = startPage + query.PageSize;

            int totalCount = DbUtil.ExecuteScalar(string.Format(@"select count(0) from ({0}) as a", sql));
            var sqlPage = string.Format(@"select a.* from ({0}) a where a.number >= {1} and a.number < {2}", sql, startPage, endPage);

            var result = DbUtil.ExecuteSqlCommand(sqlPage, DbUtil.GetDraftResult);
            return ResponseUtil.OK(new
            {
                totalCount = totalCount,
                data = result
            });
        }
        #endregion

        #region 草稿
        [HttpPost]
        public ActionResult GetDraftInfo(QueryInfo query)
        {
            //当前人
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;

            var sql = new StringBuilder();
            sql.Append(string.Format(@"SELECT ROW_NUMBER () OVER (ORDER BY a.MessageCreateTime DESC) number,
                                    b.DocumentTitle,
                                    CONVERT(varchar(100), b.ClosedOrHairTime, 20) AS ClosedOrHairTime,
                                    b.MessageId,
                                    b.WorkFlowId,
                                    --流程类型
                                    a.MessageTitle,
                                    CONVERT(varchar(100), a.MessageCreateTime, 20) AS CreateTime,
                                    CONVERT(varchar(100), a.MessageFinishTime, 20) AS FinishTime
                                     FROM WKF_Message a
                                    INNER JOIN GW_ReceiveFile b
                                    ON a.MessageID = b.MessageId
                                    WHERE a.MessageStatus = 0
                                    and a.MessageIssuedBy = '{0}' 
                                    and a.MessageType = 'file'", emplId));
            if (query.PageNumber <= 0)
            {
                query.PageNumber = 1;
            }
            if (query.PageSize <= 0)
            {
                query.PageSize = 20;
            }
            //公文标题
            if (!string.IsNullOrWhiteSpace(query.Title))
            {
                sql.Append(string.Format(@" and b.DocumentTitle like '%{0}%'", query.Title));
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
            //开始发起时间
            if (query.StartCreatedTime.HasValue)
            {
                sql.Append(string.Format(@" and a.MessageCreateTime >= '{0}'", query.StartCreatedTime));
            }
            //结束发起时间
            if (query.EndCreatedTime.HasValue)
            {
                sql.Append(string.Format(@" and a.MessageCreateTime <= '{0}'", query.EndCreatedTime));
            }
            //开始终止时间
            if (query.StartEndTime.HasValue)
            {
                sql.Append(string.Format(@" and a.MessageFinishTime >= '{0}'", query.StartEndTime));
            }
            //结束终止时间
            if (query.EndEndTime.HasValue)
            {
                sql.Append(string.Format(@" and a.MessageFinishTime <= '{0}'", query.EndEndTime));
            }
            //流程类型
            if (!string.IsNullOrWhiteSpace(query.WorkFlowId))
            {
                sql.Append(string.Format(@" and b.WorkFlowId = '{0}'", query.WorkFlowId));
            }
            //开始位置
            var startPage = query.PageSize * (query.PageNumber - 1) + 1;
            //结束位置
            var endPage = startPage + query.PageSize;

            int totalCount = DbUtil.ExecuteScalar(string.Format(@"select count(0) from ({0}) as a", sql));
            var sqlPage = string.Format(@"select a.* from ({0}) a where a.number >= {1} and a.number < {2}", sql, startPage, endPage);

            var result = DbUtil.ExecuteSqlCommand(sqlPage, DbUtil.GetDraftResult);
            return ResponseUtil.OK(new
            {
                totalCount = totalCount,
                data = result
            });
        }
        #endregion

        #region 公文详情

        #endregion

    }
}