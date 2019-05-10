using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DeptOA.Common;
using DeptOA.Models;
using System.Text;

namespace DeptOA.Controllers
{
    public class FavoriteController : Controller
    {
        #region 添加收藏
        [HttpPost]
        public ActionResult AddFavorite(FormCollection collection)
        {
            try
            {
                /*
                 * 变量定义
                 */
                // 消息
                WorkflowMgr wkfMgr = new WorkflowMgr();

                /*
                 * 参数获取
                 */
                // 消息ID
                var mid = collection["mid"];
                // 当前用户
                var employee = (User.Identity as AppkizIdentity).Employee;

                /*
                 * 参数校验
                 */
                // 消息ID
                if (string.IsNullOrEmpty(mid))
                {
                    return ResponseUtil.Error("消息ID不能为空");
                }
                var message = wkfMgr.GetMessage(mid);
                if (message == null)
                {
                    return ResponseUtil.Error("消息不存在");
                }

                /*
                 * 收藏该条公文
                 */
                // 检查是否已经收藏该条公文
                using (var db = new DefaultConnection())
                {
                    DEP_FavoriteMessage favorite = db.FavoriteMessage.Where(n => n.MessageID == mid && n.EmplID == employee.EmplID).FirstOrDefault();
                    if (favorite != null)
                    {
                        return ResponseUtil.Error("该条公文已收藏");
                    }
                    else
                    {
                        favorite = new DEP_FavoriteMessage();
                        favorite.MessageID = mid;
                        favorite.EmplID = employee.EmplID;
                        favorite.CreateTime = DateTime.Now;

                        db.FavoriteMessage.Add(favorite);
                        db.SaveChanges();

                        return ResponseUtil.OK("已收藏");
                    }
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 取消收藏公文
        [HttpPost]
        public ActionResult CancelFavorite(FormCollection collection)
        {
            /*
             * 参数获取
             */
            // 消息ID
            var mid = collection["mid"];
            // 当前用户
            var employee = (User.Identity as AppkizIdentity).Employee;

            /*
             * 参数校验
             * 不做消息是否存在的检测，防止消息不存在而无法取消收藏
             */
            // 消息ID
            if (string.IsNullOrEmpty(mid))
            {
                return ResponseUtil.Error("消息ID不能为空");
            }

            /*
             * 取消收藏
             */
            using (var db = new DefaultConnection())
            {
                DEP_FavoriteMessage favorite = db.FavoriteMessage.Where(n => n.MessageID == mid && n.EmplID == employee.EmplID).FirstOrDefault();
                if (favorite == null)
                {
                    return ResponseUtil.Error("此公文未收藏，无法取消");
                }
                else
                {
                    db.FavoriteMessage.Remove(favorite);
                    db.SaveChanges();

                    return ResponseUtil.OK("已取消");
                }
            }
        }
        #endregion

        #region 获取收藏公文列表
        public ActionResult FavoriteList(FormCollection collection)
        {
            /*
             * 变量定义
             */
            // 分页页数
            int PageNumber = 0;
            // 分页大小
            int PageSize = 10;
            // 获得今年年份
            var Year = DateTime.Now.Year.ToString();
            // 当前用户
            var employee = (User.Identity as AppkizIdentity).Employee;
            // 日期比较sql
            var dateSql = string.Empty;
            // SQL语句列表
            var statements = new List<StringBuilder>();

            /*
             * 参数获取
             */
            // 公文标题
            var Title = collection["Title"];
            // 开始日期
            var StartTime = collection["StartTime"];
            // 结束日期
            var EndTime = collection["EndTime"];
            // 分页页数
            var PageNumberStr = collection["PageNumber"];
            // 分页大小
            var PageSizeStr = collection["PageSize"];

            /*
             * 参数校验
             */
            // 分页页数
            if (!int.TryParse(PageNumberStr, out PageNumber) || PageNumber < 0)
            {
                PageNumber = 0;
            }
            // 分页大小
            if (!int.TryParse(PageSizeStr, out PageSize) || PageSize < 0)
            {
                PageSize = 10;
            }
            // 季度
            if (!string.IsNullOrEmpty(StartTime) && !string.IsNullOrEmpty(EndTime))
            {
                dateSql = string.Format(@" WHERE
	                        a.ClosedOrHairTime >= '{0}'
                            AND
	                        a.ClosedOrHairTime <= '{1}'", StartTime, EndTime);
            }

            /*
             * 构造查询语句，查询结果
             */
            /*
             * 构造SQL语句
             */
            // 根据当前用户获取对应的映射表
            var tableList = WorkflowUtil.GetTablesByUser(employee.EmplID);

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
                foreach(var tableName in tableList)
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append(string.Format(@"SELECT
	                        DISTINCT a.MessageId,
	                        a.ClosedOrHairTime,
	                        a.DocumentTitle,
	                        c.MessageIssuedBy as InitiateEmplId,
	                        d.EmplName as InitiateEmplName,
	                        c.MessageTitle,
	                        '' as MyTask,
	                        '' as ReceiveTime,
	                        a.WorkFlowId
                        FROM 
	                        {0} a
                        INNER JOIN DEP_FavoriteMessage b ON
	                        a.MessageId=b.MessageID AND a.DocumentTitle LIKE '%{1}%' AND b.EmplID='{2}'
                        INNER JOIN WKF_Message c ON
	                        c.MessageID=a.MessageId
                        INNER JOIN ORG_Employee d ON
	                        d.EmplID=c.MessageIssuedBy
                        INNER JOIN WKF_WorkflowHistory e ON
	                        e.MessageID=c.MessageID AND e.HandledBy = d.EmplID {3}", tableName, Title, employee.EmplID, dateSql));
                    statements.Add(sql);
                }
            }

            // 获得Union语句
            var unionSql = string.Join(" UNION ", statements);
            var finalSql = string.Format("select ROW_NUMBER () OVER (ORDER BY t.ClosedOrHairTime DESC) number, t.* from ({0}) t", unionSql);

            //开始位置
            var startPage = PageSize * PageNumber;
            //结束位置
            var endPage = startPage + PageSize;

            // 总数
            int totalRecouds = DbUtil.ExecuteScalar(string.Format(@"select count(0) from ({0}) as a", finalSql));
            //总页数
            var totalPages = totalRecouds % PageSize == 0 ? totalRecouds / PageSize : totalRecouds / PageSize + 1;

            var sqlPage = string.Format(@"select a.* from ({0}) a where a.number >= {1} and a.number < {2}", finalSql, startPage, endPage);

            var result = DbUtil.ExecuteSqlCommand(sqlPage, DbUtil.WKF_FavoriteList);

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
        #endregion

        #region 检查公文是否被收藏
        [HttpPost]
        public ActionResult IsFavorite(FormCollection collection)
        {
            try
            {
                /*
                 * 参数获取
                 */
                // 消息ID
                var mid = collection["mid"];
                // 当前用户
                var employee = (User.Identity as AppkizIdentity).Employee;

                /*
                 * 参数校验
                 */
                // 消息ID
                if (string.IsNullOrEmpty(mid))
                {
                    return ResponseUtil.Error("消息ID不能为空");
                }
                /*
                 * 获取收藏
                 */
                using (var db = new DefaultConnection())
                {
                    DEP_FavoriteMessage favoriteMessage = db.FavoriteMessage.Where(n => n.MessageID == mid && n.EmplID == employee.EmplID).FirstOrDefault();

                    return ResponseUtil.OK(new
                    {
                        isFavorite = (favoriteMessage != null)
                    });
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