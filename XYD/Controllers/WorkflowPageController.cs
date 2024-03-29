﻿using XYD.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using Appkiz.Apps.Workflow.Library;

namespace XYD.Controllers
{
    public class WorkflowPageController : Controller
    {

        WorkflowMgr mgr = new WorkflowMgr();

        #region 获取审批数量
        [Authorize]
        [HttpPost]
        public ActionResult GetPendingCount()
        {
            // 待办数量
            /*
             * 变量定义
             */
            // SQL语句列表
            var statements = new List<StringBuilder>();
            // 当前用户
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;
            /*
             * 构造SQL语句
             */
            // 根据当前用户获取对应的映射表
            var tableList = WorkflowUtil.GetTablesByUser(emplId);

            // 判断映射表数量
            if (tableList.Count == 0)
            {
                return new JsonNetResult(new
                {
                    TotalInfo = 0,
                });
            }
            else
            {
                foreach (var tableName in tableList)
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append(string.Format(@"SELECT
	                                                b.WorkFlowId,
	                                                COUNT ( b.WorkFlowId ) AS MessageCount,
	                                                a.MessageTitle,
	                                                f.FolderName 
                                                FROM
	                                                WKF_Message a
	                                                INNER JOIN {0} b ON a.MessageID = b.MessageId
	                                                INNER JOIN WKF_MessageHandle c ON a.MessageID = c.MessageID
	                                                INNER JOIN ORG_Employee d ON a.MessageIssuedBy = d.EmplID
	                                                INNER JOIN WKF_MessageFolder e ON b.WorkFlowId = e.MessageID
	                                                INNER JOIN WKF_Folder f ON f.FolderID = e.FolderID 
                                                WHERE
	                                                c.HandleStatus != 0 
	                                                AND a.MessageStatus NOT IN ( 0, 3 ) 
	                                                AND ( c.UserID = '{1}' OR ( c.EntrustBy = '{1}' AND c.EntrustBy <> '' ) ) 
                                                GROUP BY
	                                                b.WorkFlowId,
	                                                a.MessageTitle,
	                                                f.FolderName", tableName, emplId));
                    // 将SQL语句添加进列表
                    statements.Add(sql);
                }
            }
            // 获得Union语句
            var unionSql = string.Join(" UNION ", statements);
            var countRecords = DbUtil.ExecuteSqlCommand(unionSql, DbUtil.CountMessage);
            int totalCount = 0;
            Dictionary<string, List<object>> resultDict = new Dictionary<string, List<object>>();
            foreach(XYD_DB_Message_Count entity in countRecords)
            {
                totalCount += entity.MessageCount;
                List<object> list = new List<object>();
                if (resultDict.ContainsKey(entity.FolderName))
                {
                    list = resultDict[entity.FolderName];

                }
                else
                {
                    list = new List<object>();
                }
                list.Add(entity);
                resultDict[entity.FolderName] = list;
            }
            var resultList = resultDict.Select(n => new { Area = n.Key, Value = n.Value }).ToList();
            return new JsonNetResult(new
            {
                TotalCount = totalCount,
                Data = resultList
            });
        }
        #endregion

        #region 待处理公文
        [Authorize]
        [HttpPost]
        public ActionResult GetPendingInfo(QueryInfo query)
        {
            /*
             * 变量定义
             */
            // SQL语句列表
            var statements = new List<StringBuilder>();
            // 当前用户
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;

            /*
             * 参数校验
             */
            if (query.PageNumber <= 0)
            {
                query.PageNumber = 1;
            }
            if (query.PageSize <= 0)
            {
                query.PageSize = 20;
            }

            /*
             * 构造SQL语句
             */
            // 根据当前用户获取对应的映射表
            var tableList = WorkflowUtil.GetTablesByUser(emplId);

            // 判断映射表数量
            if(tableList.Count == 0)
            {
                return new JsonNetResult(new
                {
                    TotalInfo = 0,
                    Data = new List<object>()
                });
            }
            else
            {
                foreach (var tableName in tableList)
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append(string.Format(@"SELECT
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
                                    CONVERT(varchar(100), c.CreateTime, 20) as ReceiveTime,
                                    a.MessageIssuedBy
                                     FROM WKF_Message a
                                    INNER JOIN {0} b
                                    ON a.MessageID = b.MessageId
                                    INNER JOIN WKF_MessageHandle c
                                    ON a.MessageID = c.MessageID
                                    INNER JOIN ORG_Employee d
                                    ON a.MessageIssuedBy = d.EmplID
                                    WHERE c.HandleStatus != 0
                                    and a.MessageStatus not in (0, 3) 
                                    and (c.UserID = '{1}' or (c.EntrustBy = '{1}' and c.EntrustBy <> ''))", tableName, emplId));
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
                    // 发起人
                    if (!string.IsNullOrEmpty(query.MessageIssuedBy))
                    {
                        sql.Append(string.Format(@" and a.MessageIssuedBy = '{0}'", query.MessageIssuedBy));
                    }
                    // 将SQL语句添加进列表
                    statements.Add(sql);
                }

                // 获得Union语句
                var unionSql = string.Join(" UNION ", statements);
                var finalSql = string.Format("select ROW_NUMBER () OVER (ORDER BY t.ReceiveTime DESC) number, t.* from ({0}) t", unionSql);

                //开始位置
                var startPage = query.PageSize * (query.PageNumber - 1) + 1;
                //结束位置
                var endPage = startPage + query.PageSize;

                // 总数
                int totalRecouds = DbUtil.ExecuteScalar(string.Format(@"select count(0) from ({0}) as a", finalSql));
                //总页数
                var totalPages = totalRecouds % query.PageSize == 0 ? totalRecouds / query.PageSize : totalRecouds / query.PageSize + 1;

                var sqlPage = string.Format(@"select a.* from ({0}) a where a.number >= {1} and a.number < {2}", finalSql, startPage, endPage);

                var result = DbUtil.ExecuteSqlCommand(sqlPage, DbUtil.GetPendingResult);

                return new JsonNetResult(new {
                    TotalInfo = new
                    {
                        TotalPages = totalPages,
                        TotalRecouds = totalRecouds
                    },
                    Data = result
                });
            }
        }
        #endregion

        #region 已处理审批数量
        [Authorize]
        [HttpPost]
        public ActionResult GetDealWithCount()
        {
            /*
             * 变量定义
             */
            // SQL语句列表
            var statements = new List<StringBuilder>();
            // 当前用户
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;
            /*
             * 构造SQL语句
             */
            // 根据当前用户获取对应的映射表
            var tableList = WorkflowUtil.GetTablesByUser(emplId);

            // 判断映射表数量
            if (tableList.Count == 0)
            {
                return new JsonNetResult(new
                {
                    TotalInfo = new
                    {
                        TotalPages = 0,
                        TotalRecouds = 0
                    },
                    Data = new List<object>()
                });
            }
            else
            {
                foreach (var tableName in tableList)
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append(string.Format(@"SELECT
	                                                b.WorkFlowId,
	                                                COUNT ( b.WorkFlowId ) AS MessageCount,
	                                                a.MessageTitle,
	                                                f.FolderName 
                                                FROM
	                                                WKF_Message a
	                                                INNER JOIN {0} b ON a.MessageID = b.MessageId
	                                                INNER JOIN ( SELECT MAX ( HandledTime ) AS HandledTime, MessageID, HandledBy FROM WKF_WorkflowHistory WHERE NodeKey != 'NODE0001' GROUP BY MessageID, HandledBy ) AS d ON a.MessageID = d.MessageID
	                                                INNER JOIN ORG_Employee c ON a.MessageIssuedBy = c.EmplID
	                                                INNER JOIN WKF_MessageFolder e ON b.WorkFlowId = e.MessageID
	                                                INNER JOIN WKF_Folder f ON f.FolderID = e.FolderID 
                                                WHERE
	                                                d.HandledBy = '{1}' 
	                                                AND a.MessageStatus NOT IN ( 0, 3 ) 
                                                GROUP BY
	                                                b.WorkFlowId,
	                                                a.MessageTitle,
	                                                f.FolderName", tableName, emplId));
                    // 将SQL语句添加进列表
                    statements.Add(sql);
                }
            }
            // 获得Union语句
            var unionSql = string.Join(" UNION ", statements);
            var countRecords = DbUtil.ExecuteSqlCommand(unionSql, DbUtil.CountMessage);
            int totalCount = 0;
            Dictionary<string, List<object>> resultDict = new Dictionary<string, List<object>>();
            foreach (XYD_DB_Message_Count entity in countRecords)
            {
                totalCount += entity.MessageCount;
                List<object> list = new List<object>();
                if (resultDict.ContainsKey(entity.FolderName))
                {
                    list = resultDict[entity.FolderName];

                }
                else
                {
                    list = new List<object>();
                }
                list.Add(entity);
                resultDict[entity.FolderName] = list;
            }
            var resultList = resultDict.Select(n => new { Area = n.Key, Value = n.Value }).ToList();
            return new JsonNetResult(new
            {
                TotalCount = totalCount,
                Data = resultList
            });
        }
        #endregion

        #region 已处理过公文
        [Authorize]
        [HttpPost]
        public ActionResult GetDealWithInfo(QueryInfo query)
        {
            /*
             * 变量定义
             */
            // SQL语句列表
            var statements = new List<StringBuilder>();
            // 当前用户
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;

            /*
             * 参数校验
             */
            if (query.PageNumber <= 0)
            {
                query.PageNumber = 1;
            }
            if (query.PageSize <= 0)
            {
                query.PageSize = 20;
            }

            /*
             * 构造SQL语句
             */
            // 根据当前用户获取对应的映射表
            var tableList = WorkflowUtil.GetTablesByUser(emplId);

            // 判断映射表数量
            if (tableList.Count == 0)
            {
                return new JsonNetResult(new
                {
                    TotalInfo = new
                    {
                        TotalPages = 0,
                        TotalRecouds = 0
                    },
                    Data = new List<object>()
                });
            }
            else
            {
                foreach (var tableName in tableList)
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append(string.Format(@"SELECT
                                    b.DocumentTitle,
                                    CONVERT(varchar(100), b.ClosedOrHairTime, 20) as ClosedOrHairTime,
                                    b.MessageId,
                                    b.WorkFlowId,
                                    --流程类型
                                    a.MessageTitle,
                                    CONVERT(varchar(100), a.MessageCreateTime, 20) AS CreateTime,
                                    CONVERT(varchar(100), d.HandledTime, 20) AS ReceiveTime,
                                    a.MessageIssuedBy,
                                    c.EmplName AS InitiateEmplName,
                                    CASE
		                                WHEN a.MessageStatus = 0 THEN
		                                '草稿' 
		                                WHEN a.MessageStatus = 1 THEN
		                                '运行中' 
		                                WHEN a.MessageStatus = 2 THEN
		                                '已完成' 
		                                WHEN a.MessageStatus = 3 THEN
		                                '终止信息' ELSE NULL 
	                                END AS MessageStatusName 
                                     FROM WKF_Message a
                                    INNER JOIN {0} b
                                    ON a.MessageID = b.MessageId
                                    INNER JOIN (select MAX(HandledTime) as HandledTime,MessageID,HandledBy from WKF_WorkflowHistory 
									where NodeKey != 'NODE0001' group by MessageID,HandledBy) as d
                                    ON a.MessageID = d.MessageID
                                    INNER JOIN ORG_Employee c
                                    ON a.MessageIssuedBy = c.EmplID
                                    WHERE d.HandledBy = '{1}'  
                                    and a.MessageStatus not in (0, 3)", tableName, emplId));

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
                    // 发起人
                    if (!string.IsNullOrEmpty(query.MessageIssuedBy))
                    {
                        sql.Append(string.Format(@" and a.MessageIssuedBy = '{0}'", query.MessageIssuedBy));
                    }

                    // 将SQL语句添加进列表
                    statements.Add(sql);
                }

                // 获得Union语句
                var unionSql = string.Join(" UNION ", statements);
                var finalSql = string.Format("select ROW_NUMBER () OVER (ORDER BY t.ReceiveTime DESC) number, t.* from ({0}) t", unionSql);

                //开始位置
                var startPage = query.PageSize * (query.PageNumber - 1) + 1;
                //结束位置
                var endPage = startPage + query.PageSize;

                // 总数
                int totalRecouds = DbUtil.ExecuteScalar(string.Format(@"select count(0) from ({0}) as a", finalSql));
                //总页数
                var totalPages = totalRecouds % query.PageSize == 0 ? totalRecouds / query.PageSize : totalRecouds / query.PageSize + 1;

                var sqlPage = string.Format(@"select a.* from ({0}) a where a.number >= {1} and a.number < {2}", finalSql, startPage, endPage);

                var dealResults = DbUtil.ExecuteSqlCommand(sqlPage, DbUtil.GetDealResult);
                foreach(XYD_DealResult result in dealResults)
                {
                    result.Operation = WorkflowUtil.GetLatestOpinion(emplId, result.MessageId);
                }

                return new JsonNetResult(new
                {
                    TotalInfo = new
                    {
                        TotalPages = totalPages,
                        TotalRecouds = totalRecouds
                    },
                    Data = dealResults
                });
            }
        }
        #endregion

        #region 我发出未完成数量
        [Authorize]
        [HttpPost]
        public ActionResult GetNoCompleteCount()
        {
            /*
             * 变量定义
             */
            // SQL语句列表
            var statements = new List<StringBuilder>();
            // 当前用户
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;
            /*
             * 构造SQL语句
             */
            // 根据当前用户获取对应的映射表
            var tableList = WorkflowUtil.GetTablesByUser(emplId);

            // 判断映射表数量
            if (tableList.Count == 0)
            {
                return new JsonNetResult(new
                {
                    TotalInfo = new
                    {
                        TotalPages = 0,
                        TotalRecouds = 0
                    },
                    Data = new List<object>()
                });
            }
            else
            {
                foreach (var tableName in tableList)
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append(string.Format(@"SELECT
	                                                b.WorkFlowId,
	                                                COUNT ( b.WorkFlowId ) AS MessageCount,
	                                                a.MessageTitle,
	                                                f.FolderName 
                                                FROM
	                                                WKF_Message a
	                                                INNER JOIN {0} b ON a.MessageID = b.MessageId
	                                                INNER JOIN WKF_MessageHandle c ON a.MessageID = c.MessageID
	                                                INNER JOIN WKF_MessageFolder e ON b.WorkFlowId = e.MessageID
	                                                INNER JOIN WKF_Folder f ON f.FolderID = e.FolderID 
                                                WHERE
	                                                a.MessageStatus != 2 
	                                                AND c.HandleStatus != 0 
	                                                AND a.MessageStatus NOT IN ( 0, 3 ) 
	                                                AND a.MessageIssuedBy = '{1}' 
                                                GROUP BY
	                                                b.WorkFlowId,
	                                                a.MessageTitle,
	                                                f.FolderName", tableName, emplId));
                    // 将SQL语句添加进列表
                    statements.Add(sql);
                }
            }
            // 获得Union语句
            var unionSql = string.Join(" UNION ", statements);
            var countRecords = DbUtil.ExecuteSqlCommand(unionSql, DbUtil.CountMessage);
            int totalCount = 0;
            Dictionary<string, List<object>> resultDict = new Dictionary<string, List<object>>();
            foreach (XYD_DB_Message_Count entity in countRecords)
            {
                totalCount += entity.MessageCount;
                List<object> list = new List<object>();
                if (resultDict.ContainsKey(entity.FolderName))
                {
                    list = resultDict[entity.FolderName];

                }
                else
                {
                    list = new List<object>();
                }
                list.Add(entity);
                resultDict[entity.FolderName] = list;
            }
            var resultList = resultDict.Select(n => new { Area = n.Key, Value = n.Value }).ToList();
            return new JsonNetResult(new
            {
                TotalCount = totalCount,
                Data = resultList
            });
        }
        #endregion

        #region 我发出未完成
        [Authorize]
        [HttpPost]
        public ActionResult GetNoCompleteInfo(QueryInfo query)
        {
            /*
             * 变量定义
             */
            // SQL语句列表
            var statements = new List<StringBuilder>();
            // 当前用户
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;

            /*
             * 参数校验
             */
            if (query.PageNumber <= 0)
            {
                query.PageNumber = 1;
            }
            if (query.PageSize <= 0)
            {
                query.PageSize = 20;
            }

            /*
             * 构造SQL语句
             */
            // 根据当前用户获取对应的映射表
            var tableList = WorkflowUtil.GetTablesByUser(emplId);
            // 判断映射表数量
            if (tableList.Count == 0)
            {
                return new JsonNetResult(new
                {
                    TotalInfo = new
                    {
                        TotalPages = 0,
                        TotalRecouds = 0
                    },
                    Data = new List<object>()
                });
            }
            else
            {
                foreach (var tableName in tableList)
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append(string.Format(@"SELECT
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
                                    INNER JOIN {0} b
                                    ON a.MessageID = b.MessageId
                                    INNER JOIN WKF_MessageHandle c
                                    ON a.MessageID = c.MessageID
                                    WHERE a.MessageStatus != 2
                                    AND c.HandleStatus != 0
                                    and a.MessageStatus not in (0, 3)
                                    and a.MessageIssuedBy = '{1}'", tableName, emplId));
                    var groupInfo = string.Format(@" GROUP BY a.MessageID, a.MessageCreateTime,
                                     b.DocumentTitle,
                                     b.ClosedOrHairTime,b.MessageId,
                                     b.WorkFlowId,
									--流程类型
                                     a.MessageTitle,
                                     --环节
                                     c.NodeName,
                                     a.MessageCreateTime ");

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

                    // 将SQL语句添加进列表
                    statements.Add(sql);
                }


                //开始位置
                var startPage = query.PageSize * (query.PageNumber - 1) + 1;
                //结束位置
                var endPage = startPage + query.PageSize;

                // 获得Union语句
                var unionSql = string.Join(" UNION ", statements);
                var finalSql = string.Format("select ROW_NUMBER () OVER (ORDER BY t.CreateTime DESC) number, t.* from ({0}) t", unionSql);

                // 总数
                int totalRecouds = DbUtil.ExecuteScalar(string.Format(@"select count(0) from ({0}) as a", finalSql));
                //总页数
                var totalPages = totalRecouds % query.PageSize == 0 ? totalRecouds / query.PageSize : totalRecouds / query.PageSize + 1;

                var sqlPage = string.Format(@"select a.* from ({0}) a where a.number >= {1} and a.number < {2}", finalSql, startPage, endPage);

                var result = DbUtil.ExecuteSqlCommand(sqlPage, DbUtil.GetNoCompleteResult);

                return new JsonNetResult(new
                {
                    TotalInfo = new
                    {
                        TotalPages = totalPages,
                        TotalRecouds = totalRecouds
                    },
                    Data = result
                });
            }  
        }
        #endregion

        #region 获得我发出已完成数量
        [Authorize]
        [HttpPost]
        public ActionResult GetCompleteCount()
        {
            /*
             * 变量定义
             */
            // SQL语句列表
            var statements = new List<StringBuilder>();
            // 当前用户
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;
            /*
             * 构造SQL语句
             */
            // 根据当前用户获取对应的映射表
            var tableList = WorkflowUtil.GetTablesByUser(emplId);
            // 判断映射表数量
            if (tableList.Count == 0)
            {
                return new JsonNetResult(new
                {
                    TotalInfo = new
                    {
                        TotalPages = 0,
                        TotalRecouds = 0
                    },
                    Data = new List<object>()
                });
            }
            else
            {
                foreach (var tableName in tableList)
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append(string.Format(@"SELECT
	                                                b.WorkFlowId,
	                                                COUNT ( b.WorkFlowId ) AS MessageCount,
	                                                a.MessageTitle,
	                                                f.FolderName 
                                                FROM
	                                                WKF_Message a
	                                                INNER JOIN {0} b ON a.MessageID = b.MessageId
	                                                INNER JOIN WKF_MessageFolder e ON b.WorkFlowId = e.MessageID
	                                                INNER JOIN WKF_Folder f ON f.FolderID = e.FolderID 
                                                WHERE
	                                                a.MessageStatus in (2,3) 
	                                                AND a.MessageIssuedBy = '{1}' 
                                                GROUP BY
	                                                b.WorkFlowId,
	                                                a.MessageTitle,
	                                                f.FolderName", tableName, emplId));
                    // 将SQL语句添加进列表
                    statements.Add(sql);
                }
            }
            // 获得Union语句
            var unionSql = string.Join(" UNION ", statements);
            var countRecords = DbUtil.ExecuteSqlCommand(unionSql, DbUtil.CountMessage);
            int totalCount = 0;
            Dictionary<string, List<object>> resultDict = new Dictionary<string, List<object>>();
            foreach (XYD_DB_Message_Count entity in countRecords)
            {
                totalCount += entity.MessageCount;
                List<object> list = new List<object>();
                if (resultDict.ContainsKey(entity.FolderName))
                {
                    list = resultDict[entity.FolderName];

                }
                else
                {
                    list = new List<object>();
                }
                list.Add(entity);
                resultDict[entity.FolderName] = list;
            }
            var resultList = resultDict.Select(n => new { Area = n.Key, Value = n.Value }).ToList();
            return new JsonNetResult(new
            {
                TotalCount = totalCount,
                Data = resultList
            });
        }
        #endregion

        #region 我发出已完成
        [Authorize]
        [HttpPost]
        public ActionResult GetCompleteInfo(QueryInfo query)
        {
            /*
             * 变量定义
             */
            // SQL语句列表
            var statements = new List<StringBuilder>();
            // 当前用户
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;

            /*
             * 参数校验
             */
            if (query.PageNumber <= 0)
            {
                query.PageNumber = 1;
            }
            if (query.PageSize <= 0)
            {
                query.PageSize = 20;
            }

            /*
             * 构造SQL语句
             */
            // 根据当前用户获取对应的映射表
            var tableList = WorkflowUtil.GetTablesByUser(emplId);
            // 判断映射表数量
            if (tableList.Count == 0)
            {
                return new JsonNetResult(new
                {
                    TotalInfo = new
                    {
                        TotalPages = 0,
                        TotalRecouds = 0
                    },
                    Data = new List<object>()
                });
            }
            else
            {
                foreach (var tableName in tableList)
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append(string.Format(@"SELECT 
                                     b.DocumentTitle,
                                     CONVERT(varchar(100), b.ClosedOrHairTime, 20) AS ClosedOrHairTime,
                                     b.MessageId,
                                     b.WorkFlowId,
                                     --流程类型
                                     a.MessageTitle,
                                     CONVERT(varchar(100), a.MessageCreateTime, 20) AS CreateTime,
                                     CONVERT(varchar(100), a.MessageFinishTime, 20) AS FinishTime,
                                    CASE
		                                WHEN a.MessageStatus = 0 THEN
		                                '草稿' 
		                                WHEN a.MessageStatus = 1 THEN
		                                '运行中' 
		                                WHEN a.MessageStatus = 2 THEN
		                                '已完成' 
		                                WHEN a.MessageStatus = 3 THEN
		                                '已终止' ELSE NULL 
	                                END AS MessageStatusName 
                                     FROM WKF_Message a
                                    INNER JOIN {0} b
                                    ON a.MessageID = b.MessageId
                                    WHERE a.MessageStatus in (2,3)
                                    and a.MessageIssuedBy = '{1}'", tableName, emplId));

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

                    // 将SQL语句添加进列表
                    statements.Add(sql);
                }

                // 获得Union语句
                var unionSql = string.Join(" UNION ", statements);
                var finalSql = string.Format("select ROW_NUMBER () OVER (ORDER BY t.CreateTime DESC) number, t.* from ({0}) t", unionSql);

                //开始位置
                var startPage = query.PageSize * (query.PageNumber - 1) + 1;
                //结束位置
                var endPage = startPage + query.PageSize;

                // 总数
                int totalRecouds = DbUtil.ExecuteScalar(string.Format(@"select count(0) from ({0}) as a", finalSql));
                //总页数
                var totalPages = totalRecouds % query.PageSize == 0 ? totalRecouds / query.PageSize : totalRecouds / query.PageSize + 1;

                var sqlPage = string.Format(@"select a.* from ({0}) a where a.number >= {1} and a.number < {2}", finalSql, startPage, endPage);

                var result = DbUtil.ExecuteSqlCommand(sqlPage, DbUtil.GetCompleteResult);

                return new JsonNetResult(new
                {
                    TotalInfo = new
                    {
                        TotalPages = totalPages,
                        TotalRecouds = totalRecouds
                    },
                    Data = result
                });
            }
        }
        #endregion

        #region 我的申请
        [Authorize]
        [HttpPost]
        public ActionResult GetMyApply(QueryInfo query)
        {
            /*
             * 变量定义
             */
            // SQL语句列表
            var statements = new List<StringBuilder>();
            // 当前用户
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;

            /*
             * 参数校验
             */
            if (query.PageNumber <= 0)
            {
                query.PageNumber = 1;
            }
            if (query.PageSize <= 0)
            {
                query.PageSize = 20;
            }

            /*
             * 构造SQL语句
             */
            // 根据当前用户获取对应的映射表
            var tableList = WorkflowUtil.GetTablesByUser(emplId);
            // 判断映射表数量
            if (tableList.Count == 0)
            {
                return new JsonNetResult(new
                {
                    TotalInfo = new
                    {
                        TotalPages = 0,
                        TotalRecouds = 0
                    },
                    Data = new List<object>()
                });
            }
            else
            {
                foreach (var tableName in tableList)
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append(string.Format(@"SELECT 
                                     b.DocumentTitle,
                                     CONVERT(varchar(100), b.ClosedOrHairTime, 20) AS ClosedOrHairTime,
                                     b.MessageId,
                                     b.WorkFlowId,
                                     --流程类型
                                     a.MessageTitle,
                                     CONVERT(varchar(100), a.MessageCreateTime, 20) AS CreateTime,
                                     CONVERT(varchar(100), a.MessageFinishTime, 20) AS FinishTime,
                                     a.MessageStatus,
                                     a.MessageIssuedBy
                                     FROM WKF_Message a
                                    INNER JOIN {0} b
                                    ON a.MessageID = b.MessageId
                                    WHERE a.MessageIssuedBy = '{1}'
                                    and a.MessageStatus != 3 ", tableName, emplId));

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

                    // 将SQL语句添加进列表
                    statements.Add(sql);
                }

                // 获得Union语句
                var unionSql = string.Join(" UNION ", statements);
                var finalSql = string.Format("select ROW_NUMBER () OVER (ORDER BY t.CreateTime DESC) number, t.* from ({0}) t", unionSql);

                //开始位置
                var startPage = query.PageSize * (query.PageNumber - 1) + 1;
                //结束位置
                var endPage = startPage + query.PageSize;

                // 总数
                int totalRecouds = DbUtil.ExecuteScalar(string.Format(@"select count(0) from ({0}) as a", finalSql));
                //总页数
                var totalPages = totalRecouds % query.PageSize == 0 ? totalRecouds / query.PageSize : totalRecouds / query.PageSize + 1;

                var sqlPage = string.Format(@"select a.* from ({0}) a where a.number >= {1} and a.number < {2}", finalSql, startPage, endPage);

                var result = DbUtil.ExecuteSqlCommand(sqlPage, DbUtil.GetMyApplyResult);

                return new JsonNetResult(new
                {
                    TotalInfo = new
                    {
                        TotalPages = totalPages,
                        TotalRecouds = totalRecouds
                    },
                    Data = result
                });
            }
        }
        #endregion

        #region 草稿数量
        [Authorize]
        [HttpPost]
        public ActionResult GetDraftCount()
        {
            /*
             * 变量定义
             */
            // SQL语句列表
            var statements = new List<StringBuilder>();
            // 当前用户
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;
            /*
             * 构造SQL语句
             */
            // 根据当前用户获取对应的映射表
            var tableList = WorkflowUtil.GetTablesByUser(emplId);
            // 判断映射表数量
            if (tableList.Count == 0)
            {
                return new JsonNetResult(new
                {
                    TotalInfo = new
                    {
                        TotalPages = 0,
                        TotalRecouds = 0
                    },
                    Data = new List<object>()
                });
            }
            else
            {
                foreach (var tableName in tableList)
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append(string.Format(@"SELECT
	                                                b.WorkFlowId,
	                                                COUNT ( b.WorkFlowId ) AS MessageCount,
	                                                a.MessageTitle,
	                                                f.FolderName 
                                                FROM
	                                                WKF_Message a
	                                                INNER JOIN {0} b ON a.MessageID = b.MessageId
	                                                INNER JOIN WKF_MessageFolder e ON b.WorkFlowId = e.MessageID
	                                                INNER JOIN WKF_Folder f ON f.FolderID = e.FolderID 
                                                WHERE
	                                                a.MessageStatus = 0 
	                                                AND a.MessageIssuedBy = '{1}' 
	                                                AND a.MessageType = 'file' 
                                                GROUP BY
	                                                b.WorkFlowId,
	                                                a.MessageTitle,
	                                                f.FolderName", tableName, emplId));
                    // 将SQL语句添加进列表
                    statements.Add(sql);
                }
            }
            // 获得Union语句
            var unionSql = string.Join(" UNION ", statements);
            var countRecords = DbUtil.ExecuteSqlCommand(unionSql, DbUtil.CountMessage);
            int totalCount = 0;
            Dictionary<string, List<object>> resultDict = new Dictionary<string, List<object>>();
            foreach (XYD_DB_Message_Count entity in countRecords)
            {
                totalCount += entity.MessageCount;
                List<object> list = new List<object>();
                if (resultDict.ContainsKey(entity.FolderName))
                {
                    list = resultDict[entity.FolderName];

                }
                else
                {
                    list = new List<object>();
                }
                list.Add(entity);
                resultDict[entity.FolderName] = list;
            }
            var resultList = resultDict.Select(n => new { Area = n.Key, Value = n.Value }).ToList();
            return new JsonNetResult(new
            {
                TotalCount = totalCount,
                Data = resultList
            });
        }
        #endregion

        #region 草稿
        [Authorize]
        [HttpPost]
        public ActionResult GetDraftInfo(QueryInfo query)
        {
            /*
             * 变量定义
             */
            // SQL语句列表
            var statements = new List<StringBuilder>();
            // 当前用户
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;

            /*
             * 参数校验
             */
            if (query.PageNumber <= 0)
            {
                query.PageNumber = 1;
            }
            if (query.PageSize <= 0)
            {
                query.PageSize = 20;
            }

            /*
             * 构造SQL语句
             */
            // 根据当前用户获取对应的映射表
            var tableList = WorkflowUtil.GetTablesByUser(emplId);
            // 判断映射表数量
            if (tableList.Count == 0)
            {
                return new JsonNetResult(new
                {
                    TotalInfo = new
                    {
                        TotalPages = 0,
                        TotalRecouds = 0
                    },
                    Data = new List<object>()
                });
            }
            else
            {
                foreach (var tableName in tableList)
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append(string.Format(@"SELECT 
                                    b.DocumentTitle,
                                    CONVERT(varchar(100), b.ClosedOrHairTime, 20) AS ClosedOrHairTime,
                                    b.MessageId,
                                    b.WorkFlowId,
                                    --流程类型
                                    a.MessageTitle,
                                    CONVERT(varchar(100), a.MessageCreateTime, 20) AS CreateTime,
                                    CONVERT(varchar(100), a.MessageFinishTime, 20) AS FinishTime
                                     FROM WKF_Message a
                                    INNER JOIN {0} b
                                    ON a.MessageID = b.MessageId
                                    WHERE a.MessageStatus = 0
                                    and a.MessageIssuedBy = '{1}' 
                                    and a.MessageType = 'file'", tableName, emplId));

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

                    // 将SQL语句添加进列表
                    statements.Add(sql);
                }

                // 获得Union语句
                var unionSql = string.Join(" UNION ", statements);
                var finalSql = string.Format("select ROW_NUMBER () OVER (ORDER BY t.CreateTime DESC) number, t.* from ({0}) t", unionSql);

                //开始位置
                var startPage = query.PageSize * (query.PageNumber - 1) + 1;
                //结束位置
                var endPage = startPage + query.PageSize;

                // 总数
                int totalRecouds = DbUtil.ExecuteScalar(string.Format(@"select count(0) from ({0}) as a", finalSql));
                //总页数
                var totalPages = totalRecouds % query.PageSize == 0 ? totalRecouds / query.PageSize : totalRecouds / query.PageSize + 1;

                var sqlPage = string.Format(@"select a.* from ({0}) a where a.number >= {1} and a.number < {2}", finalSql, startPage, endPage);

                var result = DbUtil.ExecuteSqlCommand(sqlPage, DbUtil.GetDraftResult);

                return new JsonNetResult(new
                {
                    TotalInfo = new
                    {
                        TotalPages = totalPages,
                        TotalRecouds = totalRecouds
                    },
                    Data = result
                });
            }
            
        }
        #endregion

        #region 待处理总数
        [Authorize]
        [HttpPost]
        public ActionResult TodoNumber()
        {
            /*
             * 变量定义
             */
            // SQL语句列表
            var statements = new List<StringBuilder>();
            // 当前用户
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;

            /*
             * 构造SQL语句
             */
            // 根据当前用户获取对应的映射表
            var tableList = WorkflowUtil.GetTablesByUser(emplId);

            // 判断映射表数量
            if (tableList.Count == 0)
            {
                return new JsonNetResult(new
                {
                    TotalInfo = 0,
                    Data = new List<object>()
                });
            }
            else
            {
                foreach (var tableName in tableList)
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append(string.Format(@"SELECT
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
                                    INNER JOIN {0} b
                                    ON a.MessageID = b.MessageId
                                    INNER JOIN WKF_MessageHandle c
                                    ON a.MessageID = c.MessageID
                                    INNER JOIN ORG_Employee d
                                    ON a.MessageIssuedBy = d.EmplID
                                    WHERE c.HandleStatus != 0
                                    and a.MessageStatus != 3 
                                    and (c.UserID = '{1}' or (c.EntrustBy = '{1}' and c.EntrustBy <> ''))", tableName, emplId));

                    // 将SQL语句添加进列表
                    statements.Add(sql);
                }

                // 获得Union语句
                var unionSql = string.Join(" UNION ", statements);
                var finalSql = string.Format("select ROW_NUMBER () OVER (ORDER BY t.ReceiveTime DESC) number, t.* from ({0}) t", unionSql);

                // 总数
                int totalRecouds = DbUtil.ExecuteScalar(string.Format(@"select count(0) from ({0}) as a", finalSql));

                return Json(new
                {
                    Succeed = true,
                    Data = new
                    {
                        Total = totalRecouds
                    }
                });
            }
                    
        }
        #endregion

        #region 获得模板列表
        [Authorize]
        public ActionResult GetTempList()
        {
            /*
             * 变量定义
             */
            // 当前用户
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;

            /*
             * 根据用户获得该用户部门对应的公文
             */
            var templateList = WorkflowUtil.GetViewTemplatesByUser(User.Identity.Name);

            return ResponseUtil.OK(templateList);
        }
        #endregion

        #region 公文搜索
        [Authorize]
        public ActionResult GetWorkflowQueryInfo(Entity.WorkflowQuery query)
        {
            /*
             * 变量定义
             */
            // SQL语句列表
            var statements = new List<StringBuilder>();
            // 当前用户
            var emplId = (User.Identity as Appkiz.Library.Security.Authentication.AppkizIdentity).Employee.EmplID;

            /*
             * 参数校验
             */
            if (query.PageNumber <= 0)
            {
                query.PageNumber = 1;
            }
            if (query.PageSize <= 0)
            {
                query.PageSize = 20;
            }

            /*
             * 构造SQL语句
             */
            // 根据当前用户获取对应的映射表
            var tableList = WorkflowUtil.GetTablesByUser(emplId);

            // 判断映射表数量
            if (tableList.Count == 0)
            {
                return new JsonNetResult(new
                {
                    TotalInfo = 0,
                    Data = new List<object>()
                });
            }
            else
            {
                foreach (var tableName in tableList)
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append(string.Format(@"SELECT
	                                                a.MessageId,
	                                                a.WorkFlowId,
	                                                CONVERT(varchar(100), a.ClosedOrHairTime, 20) as ClosedOrHairTime,
	                                                a.DocumentTitle,
	                                                b.MessageIssuedBy,
	                                                c.EmplName as MessageIssuedName,
	                                                b.MessageIssuedDept,
	                                                d.DeptName as MessageIssuedDeptName,
	                                                d.DeptName,
	                                                b.MessageTitle,
	                                                b.MessageStatus,
	                                                CONVERT(varchar(100), b.MessageCreateTime, 20) as MessageCreateTime,
                                                CASE
		                                                WHEN b.MessageStatus = 0 THEN
		                                                '草稿' 
		                                                WHEN b.MessageStatus = 1 THEN
		                                                '运行中' 
		                                                WHEN b.MessageStatus = 2 THEN
		                                                '已完成' 
		                                                WHEN b.MessageStatus = 3 THEN
		                                                '终止信息' ELSE NULL 
	                                                END AS MessageStatusName 
                                                FROM
	                                                {0} a
	                                                INNER JOIN WKF_Message b ON b.MessageStatus != 3 
	                                                AND b.FromTemplate = a.WorkFlowId 
	                                                AND a.MessageId = b.MessageID
	                                                INNER JOIN ORG_Employee c ON b.MessageIssuedBy = c.EmplID
	                                                INNER JOIN ORG_Department d ON b.MessageIssuedDept = d.DeptID
                                                WHERE
	                                                1 = 1", tableName));
                    // 标题搜索
                    if (!string.IsNullOrEmpty(query.QueryCondition))
                    {
                        sql.Append(string.Format(@" and a.DocumentTitle like '%{0}%'", query.QueryCondition));
                    }
                    //流程类型
                    if (!string.IsNullOrWhiteSpace(query.WorkFlowId))
                    {
                        sql.Append(string.Format(@" and a.WorkflowId = '{0}'", query.WorkFlowId));
                    }
                    //流程状态
                    if (query.MessageStatus.HasValue)
                    {
                        sql.Append(string.Format(@" and b.MessageStatus = '{0}'", query.MessageStatus.Value));
                    }
                    //开始收发文时间
                    if (query.StartClosedOrHairTime.HasValue)
                    {
                        sql.Append(string.Format(@" and a.ClosedOrHairTime >= '{0}'", query.StartClosedOrHairTime.Value));
                    }
                    //结束收发文时间
                    if (query.EndClosedOrHairTime.HasValue)
                    {
                        sql.Append(string.Format(@" and a.ClosedOrHairTime <= '{0}'", query.EndClosedOrHairTime.Value));
                    }
                    //发起部门
                    if (!string.IsNullOrWhiteSpace(query.MessageIssuedDept))
                    {
                        sql.Append(string.Format(@" and b.MessageIssuedDept = '{0}'", query.MessageIssuedDept));
                    }
                    //发起人
                    if (!string.IsNullOrWhiteSpace(query.MessageIssuedBy))
                    {
                        sql.Append(string.Format(@" and b.MessageIssuedBy = '{0}'", query.MessageIssuedBy));
                    }
                    // 将SQL语句添加进列表
                    statements.Add(sql);
                }

                // 获得Union语句
                var unionSql = string.Join(" UNION ", statements);
                var finalSql = string.Format("select ROW_NUMBER () OVER (ORDER BY t.MessageCreateTime DESC) number, t.* from ({0}) t", unionSql);

                //开始位置
                var startPage = query.PageSize * (query.PageNumber - 1) + 1;
                //结束位置
                var endPage = startPage + query.PageSize;

                // 总数
                int totalRecouds = DbUtil.ExecuteScalar(string.Format(@"select count(0) from ({0}) as a", finalSql));
                //总页数
                var totalPages = totalRecouds % query.PageSize == 0 ? totalRecouds / query.PageSize : totalRecouds / query.PageSize + 1;

                var sqlPage = string.Format(@"select a.* from ({0}) a where a.number >= {1} and a.number < {2}", finalSql, startPage, endPage);

                var result = DbUtil.ExecuteSqlCommand(sqlPage, DbUtil.GetSearchResult);

                return new JsonNetResult(new
                {
                    TotalInfo = new
                    {
                        TotalPages = totalPages,
                        TotalRecouds = totalRecouds
                    },
                    Data = result
                });
            }
        }
        #endregion
    }
}