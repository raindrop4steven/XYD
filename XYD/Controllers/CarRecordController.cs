using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using Appkiz.Library.Security.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Models;

namespace XYD.Controllers
{
    public class CarRecordController : Controller
    {
        OrgMgr orgMgr = new OrgMgr();
        WorkflowMgr mgr = new WorkflowMgr();

        #region 增加车辆记录
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Add(string mid, string opinion, XYD_CarRecord model)
        {
            try
            {
                if (opinion != DEP_Constants.Audit_Operate_Type_Agree)
                {
                    return ResponseUtil.OK("没有同意，无需添加申请记录");
                }
                var message = mgr.GetMessage(mid);
                var driver = orgMgr.FindEmployee("EmplName=@EmplName", new System.Collections.Hashtable()
              {
                {
                  "@EmplName",
                  model.DriverID
                }
              }, string.Empty, 0, 1 ).FirstOrDefault();
                using (var db = new DefaultConnection())
                {
                    var record = db.CarRecord.Where(n => n.MessageID == mid).FirstOrDefault();
                    if (record == null)
                    {
                        model.MessageID = mid;
                        model.ApplyUserID = message.MessageIssuedBy;
                        model.ApplyUser = orgMgr.GetEmployee(model.ApplyUserID).EmplName;
                        model.DriverID = driver.EmplID;
                        model.CreateTime = DateTime.Now;
                        model.UpdateTime = DateTime.Now;
                        model.Status = DEP_Constants.CAR_MILES_UNFINISH;
                        db.CarRecord.Add(model);
                    }
                    else
                    {
                        model.DriverID = driver.EmplID;
                        model.UpdateTime = DateTime.Now;
                        CommonUtils.CopyProperties<XYD_CarRecord>(model, record);
                    }
                    db.SaveChanges();
                    return ResponseUtil.OK("添加用车记录成功");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 列表
        [Authorize]
        public ActionResult List(string status, int Page, int Size)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                using (var db = new DefaultConnection())
                {
                    var list = db.CarRecord.Where(n => n.DriverID == employee.EmplID);
                    if (!string.IsNullOrEmpty(status))
                    {
                        list = list.Where(n => n.Status == status);
                    }
                    // 记录总数
                    var totalCount = list.Count();
                    // 记录总页数
                    var totalPage = (int)Math.Ceiling((float)totalCount / Size);
                    var results = list.OrderByDescending(n => n.CreateTime).Skip(Page * Size).Take(Size).ToList();
                    foreach (var record in results)
                    {
                        var statusName = string.Empty;
                        if (record.Status == DEP_Constants.CAR_MILES_UNFINISH)
                        {
                            statusName = "待填写";
                        }
                        else
                        {
                            statusName = "已填写";
                        }
                        record.Status = statusName;
                    }
                    return ResponseUtil.OK(new
                    {
                        records = results,
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

        #region 填写起始公里数
        [Authorize]
        public ActionResult CompleteMiles(XYD_CarRecord record)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var entity = db.CarRecord.Where(n => n.ID == record.ID).FirstOrDefault();
                    if (entity == null)
                    {
                        return ResponseUtil.Error("记录不存在");
                    }
                    entity.StartMiles = record.StartMiles;
                    entity.EndMiles = record.EndMiles;
                    entity.Miles = record.EndMiles - record.StartMiles;
                    entity.Status = DEP_Constants.CAR_MILES_FINISH;
                    db.SaveChanges();
                    return ResponseUtil.OK("记录成功");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 统计公里数
        [Authorize]
        public ActionResult Summary(DateTime BeginDate, DateTime EndDate)
        {
            try
            {
                EndDate = CommonUtils.EndOfDay(EndDate);
                using (var db = new DefaultConnection())
                {
                    var list = db.CarRecord.Where(n => n.Status == DEP_Constants.CAR_MILES_FINISH && n.CreateTime >= BeginDate.Date && n.CreateTime <= EndDate)
                        .GroupBy(n => n.ApplyDept)
                        .Select(n => new
                        {
                            ApplyDept = n.FirstOrDefault().ApplyDept,
                            Miles = n.Sum(x => x.Miles)
                        }).ToList();
                    return ResponseUtil.OK(list);
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