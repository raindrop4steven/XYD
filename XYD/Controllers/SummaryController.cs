using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Models;

namespace XYD.Controllers
{
    public class SummaryController : Controller
    {
        OrgMgr orgMgr = new OrgMgr();
        WorkflowMgr mgr = new WorkflowMgr();

        #region 列表
        public ActionResult List(DateTime BeginDate, DateTime EndDate, int Page, int Size, string Area)
        {
            try
            {
                // 记录列表
                var AreaName = string.Empty;
                if (!string.IsNullOrEmpty(Area))
                {
                    if (Area == "001")
                    {
                        AreaName = "无锡";
                    }
                    else
                    {
                        AreaName = "上海";
                    }
                }
                BeginDate = CommonUtils.StartOfDay(BeginDate);
                EndDate = CommonUtils.EndOfDay(EndDate);
                using (var db = new DefaultConnection())
                {
                    var list = db.Voucher.Where(n => n.CreateTime >= BeginDate.Date && n.CreateTime <= EndDate && n.MessageID != DEP_Constants.INVOICE_WORKFLOW_ID).GroupBy(n => n.ApplyUser).ToList();
                    if (!string.IsNullOrEmpty(AreaName))
                    {
                        list = list.Where(n => OrgUtil.CheckRole(n.FirstOrDefault().ApplyUser, AreaName)).ToList();
                    }
                      
                    var filterList = list.Select(n => new 
                        {
                            userId = n.FirstOrDefault().ApplyUser,
                            userName = orgMgr.GetEmployee(n.FirstOrDefault().ApplyUser).EmplName,
                            deptName = orgMgr.GetEmployee(n.FirstOrDefault().ApplyUser).DeptName,
                            amount = n.Sum(x => decimal.Parse(x.TotalAmount))
                        }).OrderByDescending(n => n.amount).ToList();
                    var totalCount = filterList.Count();
                    var results = filterList.Skip(Page * Size).Take(Size);
                    var totalPage = (int)Math.Ceiling((float)totalCount / Size);
                    return ResponseUtil.OK(new
                    {
                        results = results,
                        meta = new
                        {
                            current_page = Page,
                            total_page = totalPage,
                            current_count = Page * Size + results.Count(),
                            total_count = totalCount,
                            per_page = Size
                        }
                    });
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 报销统计详情
        [Authorize]
        public ActionResult ExpenseDetail(string EmplID, string Category, DateTime? StartDate, DateTime? EndDate, int Page=0, int Size=10)
        {
            var whereBudier = new StringBuilder();
            whereBudier.Append(string.Format(" AND a.ApplyUser = '{0}'", EmplID));
            whereBudier.Append(string.Format(" AND b.MessageTitle LIKE '{0}%'", Category));
            if (StartDate != null)
            {
                whereBudier.Append(string.Format(" AND a.CreateTime >= '{0}'", CommonUtils.StartOfDay(StartDate.Value)));
            }
            if (EndDate != null)
            {
                whereBudier.Append(string.Format(" AND a.CreateTime <= '{0}'", CommonUtils.EndOfDay(EndDate.Value)));
            }
            var sql = string.Format(@"SELECT
	                        a.MessageID,
	                        b.MessageTitle,
	                        a.CreateTime,
	                        a.Sn,
	                        a.TotalAmount,
	                        a.ApplyUser,
	                        c.EmplName,
	                        d.DeptName
                        FROM
	                        XYD_Voucher a
	                        LEFT JOIN WKF_Message b ON a.MessageID = b.MessageID
	                        LEFT JOIN ORG_Employee c ON a.ApplyUser = c.EmplID
	                        LEFT JOIN ORG_Department d ON c.DeptID = d.DeptID
                        WHERE
	                        a.MessageID != 'Invoice' 
                            {0}", whereBudier.ToString());
            //开始位置
            var startPage = Size * Page;
            //结束位置
            var endPage = startPage + Size;

            // 获得Union语句
            var finalSql = string.Format("select ROW_NUMBER () OVER (ORDER BY t.CreateTime DESC) number, t.* from ({0}) t", sql);

            // 总数
            int totalRecouds = DbUtil.ExecuteScalar(string.Format(@"select count(0) from ({0}) as a", finalSql));
            //总页数
            var totalPages = totalRecouds % Size == 0 ? totalRecouds / Size : totalRecouds / Size + 1;

            var sqlPage = string.Format(@"select a.* from ({0}) a where a.number >= {1} and a.number < {2}", finalSql, startPage, endPage);

            var results = DbUtil.ExecuteSqlCommand(sqlPage, DbUtil.ExpenseDetailHandler);
            return ResponseUtil.OK(new
            {
                results = results,
                meta = new
                {
                    current_page = Page,
                    total_page = totalPages,
                    current_count = Page * Size + results.Count(),
                    total_count = totalRecouds,
                    per_page = Size
                }
            });

        }
        #endregion
    }
}