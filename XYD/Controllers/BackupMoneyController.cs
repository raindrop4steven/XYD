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
    public class BackupMoneyController : Controller
    {
        WorkflowMgr mgr = new WorkflowMgr();
        SheetMgr sheetMgr = new SheetMgr();
        OrgMgr orgMgr = new OrgMgr();

        #region 流程导入备用金
        public ActionResult Import(string mid, XYD_BackupMoney model)
        {
            try
            {
                // 根据流程ID获得发起人
                var message = mgr.GetMessage(mid);
                var applyUser = orgMgr.GetEmployee(message.MessageIssuedBy);
                model.MessageID = mid;
                model.EmplID = applyUser.EmplID;
                model.DeptID = applyUser.DeptID;
                model.EmplName = applyUser.EmplName;
                model.Status = DEP_Constants.MONEY_WAIT_PAY;
                using (var db = new DefaultConnection())
                {
                    db.BackupMoney.Add(model);
                    db.SaveChanges();
                    return ResponseUtil.OK("备用金添加成功");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 查询备用金
        [Authorize]
        public ActionResult List(string DeptID, string Status, DateTime? BeginTime, DateTime? EndTime, int Page = 0, int Size = 10)
        {
            try
            {
                var db = new DefaultConnection();
                var query = db.BackupMoney.Where(n => true);
                if (!string.IsNullOrEmpty(DeptID))
                {
                    query = query.Where(n => n.DeptID == DeptID);
                }
                if (!string.IsNullOrEmpty(Status))
                {
                    query = query.Where(n => n.Status == Status);
                }
                if (BeginTime != null)
                {
                    query = query.Where(n => n.PaybackTime >= BeginTime.Value.Date);
                }
                if (EndTime != null)
                {
                    EndTime = CommonUtils.EndOfDay(EndTime.Value);
                    query = query.Where(n => n.PaybackTime <= EndTime);
                }
                var dataQuery = query;
                decimal sumQuery = query.ToList().Select(n => n.Amount).Sum();
                int totalCount = query.Count();
                var list = dataQuery.OrderByDescending(n => n.PaybackTime).Skip(Page * Size).Take(Size);
                var sumAmount = sumQuery;
                var totalPage = (int)Math.Ceiling((float)totalCount / Size);
                return ResponseUtil.OK(new
                {
                    sumAmount = sumAmount,
                    records = list,
                    meta = new
                    {
                        current_page = Page,
                        total_page = totalPage,
                        current_count = Page * Size + list.Count(),
                        total_count = totalCount,
                        per_page = Size
                    }
                });
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 备用金归还
        [Authorize]
        public ActionResult Repay(int ID)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var money = db.BackupMoney.Where(n => n.ID == ID).FirstOrDefault();
                    if (money == null)
                    {
                        return ResponseUtil.Error("记录不存在");
                    }
                    else
                    {
                        money.Status = DEP_Constants.MONEY_REPAY;
                        db.SaveChanges();
                        return ResponseUtil.OK("操作成功");
                    }
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 状态列表
        [Authorize]
        public ActionResult StatusList()
        {
            try
            {
                var statusList = new List<object>
                {
                    new
                    {
                        key = DEP_Constants.MONEY_WAIT_PAY,
                        value = "未还款"
                    },
                    new
                    {
                        key = DEP_Constants.MONEY_REPAY,
                        value = "已还款"
                    },
                    new
                    {
                        key = DEP_Constants.MONEY_DELAYED,
                        value = "已延期"
                    }
                };
                return ResponseUtil.OK(statusList);
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion
    }
}