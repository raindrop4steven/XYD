using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Entity;
using static XYD.Common.DEP_Constants;

namespace XYD.Controllers
{
    public class CalendarController : Controller
    {
        #region 日历详情
        [Authorize]
        public ActionResult MonthData(DateTime StartDate, DateTime EndDate)
        {
            try
            {
                List<XYD_CalendarEntity> dates = new List<XYD_CalendarEntity>();
                Dictionary<CALENDAR_TYPE, int> summary = new Dictionary<CALENDAR_TYPE, int>() {
                    {CALENDAR_TYPE.Holiday, 0 },
                    {CALENDAR_TYPE.Adjust, 0 },
                    {CALENDAR_TYPE.Rest, 0 },
                    {CALENDAR_TYPE.Work, 0 },
                    {CALENDAR_TYPE.Late, 0 },
                    {CALENDAR_TYPE.LeaveEarly, 0 },
                    {CALENDAR_TYPE.Absent, 0 },
                    {CALENDAR_TYPE.Leave, 0 }
                };
                // 获得指定年份
                int currentYear = StartDate.Year;
                var calendar = CalendarUtil.GetPlanByYear(currentYear);
                var holidayDict = CalendarUtil.GetHolidays(calendar);
                var adjustDict = CalendarUtil.GetAdjusts(calendar);
                for(DateTime d = StartDate; d <= EndDate; d =  d.AddDays(1))
                {
                    var date = d.ToString("yyyy-MM-dd");
                    var entity = new XYD_CalendarEntity();
                    entity.Date = date;
                    // 首先判断是否是节日
                    if (holidayDict.ContainsKey(date))
                    {
                        entity.Name = holidayDict[date];
                        entity.Type = DEP_Constants.CALENDAR_TYPE.Holiday;
                    } else if (adjustDict.ContainsKey(date))
                    {
                        entity.Name = adjustDict[date];
                        entity.Type = DEP_Constants.CALENDAR_TYPE.Adjust;
                    } else if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                    {
                        entity.Name = "休息";
                        entity.Type = DEP_Constants.CALENDAR_TYPE.Rest;
                    } else
                    {
                        entity.Name = "上班";
                        entity.Type = DEP_Constants.CALENDAR_TYPE.Work;
                    }
                    summary[entity.Type] += 1;
                    dates.Add(entity);
                }
                return ResponseUtil.OK(new { 
                    dates = dates,
                    summary = summary
                });
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion
    }
}