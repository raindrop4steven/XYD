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
                            amount = n.Sum(x => float.Parse(x.TotalAmount))
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
    }
}