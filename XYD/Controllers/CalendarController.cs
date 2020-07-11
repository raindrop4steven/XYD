using Appkiz.Library.Security;
using Appkiz.Library.Security.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Entity;
using XYD.Models;
using static XYD.Common.DEP_Constants;

namespace XYD.Controllers
{
    public class CalendarController : Controller
    {
        #region 日历详情
        [Authorize]
        public ActionResult MonthData(DateTime currentMonth)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                OrgMgr orgMgr = new OrgMgr();
                // 获得指定年份放假和调休计划
                var StartDate = new DateTime(currentMonth.Year, currentMonth.Month, 1);
                DateTime EndDate;
                if (currentMonth.Month == DateTime.Now.Month) // 本月数据截至到今天
                {
                    EndDate = DateTime.Now;
                }
                else // 本月前和本月后
                {
                    EndDate = CommonUtils.EndOfDay(StartDate.AddMonths(1).AddDays(-1));
                }
                var result = CalendarUtil.CaculateUserCalendarDetail(employee, StartDate, EndDate, true);
                return ResponseUtil.OK(result);
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion
    }
}