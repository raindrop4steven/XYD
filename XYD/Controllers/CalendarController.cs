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
                var db = new DefaultConnection();

                List<XYD_CalendarEntity> dates = new List<XYD_CalendarEntity>();
                Dictionary<CALENDAR_TYPE, int> summary = new Dictionary<CALENDAR_TYPE, int>() {
                    {CALENDAR_TYPE.Holiday, 0 },
                    {CALENDAR_TYPE.Adjust, 0 },
                    {CALENDAR_TYPE.Rest, 0 },
                    {CALENDAR_TYPE.Work, 0 },
                    {CALENDAR_TYPE.Late, 0 },
                    {CALENDAR_TYPE.LeaveEarly, 0 },
                    {CALENDAR_TYPE.Absent, 0 },
                    {CALENDAR_TYPE.Leave, 0 },
                    {CALENDAR_TYPE.BizTrp, 0 }
                };
                // 获得指定年份放假和调休计划
                var StartDate = new DateTime(currentMonth.Year, currentMonth.Month, 1);
                var EndDate = CommonUtils.EndOfDay(StartDate.AddMonths(1).AddDays(-1));

                int currentYear = currentMonth.Year;
                var calendar = CalendarUtil.GetPlanByYear(currentYear);
                var holidayDict = CalendarUtil.GetHolidays(calendar);
                var adjustDict = CalendarUtil.GetAdjusts(calendar);
                // 获得考勤记录
                var lastDayTime = CommonUtils.EndOfDay(EndDate);
                var attenceRecords = db.Attence.Where(n => n.EmplNo == employee.EmplNO && n.StartTime >= StartDate.Date && n.EndTime <= lastDayTime).OrderBy(n => n.StartTime).ToList();
                // 获得请假记录
                var leaveRecord = db.LeaveRecord.Where(n => n.EmplID == employee.EmplID && n.StartDate >= StartDate.Date && n.EndDate <= lastDayTime).OrderBy(n => n.StartDate).ToList();
                // 获得出差记录
                var bizTripRecord = db.BizTrip.Where(n => n.EmplID == employee.EmplID && n.StartDate >= StartDate.Date && n.EndDate <= lastDayTime).OrderBy(n => n.StartDate).ToList();
                // 获得对应城市工作时间配置
                var workArea = OrgUtil.GetWorkArea(employee.EmplID);
                var sysConfig = db.SystemConfig.Where(n => n.Area == workArea).FirstOrDefault();
                // 今天
                var today = DateTime.Now.Date;
                // 判断每一天状态
                for(DateTime d = StartDate; d <= EndDate; d =  d.AddDays(1))
                {
                    var date = d.ToString("yyyy-MM-dd");
                    var entity = new XYD_CalendarEntity();
                    entity.Date = date;
                    // 首先判断是否是节日
                    if (holidayDict.ContainsKey(date))
                    {
                        entity.Name = holidayDict[date];
                        entity.Type = CALENDAR_TYPE.Holiday;
                    } else if (adjustDict.ContainsKey(date))
                    {
                        entity.Name = adjustDict[date];
                        entity.Type = CALENDAR_TYPE.Adjust;
                    } else if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                    {
                        entity.Name = "休息";
                        entity.Type = CALENDAR_TYPE.Rest;
                    } else
                    {
                        // 超过今天的，不用做判断，直接上班
                        if (d > today)
                        {
                            entity.Name = "上班";
                            entity.Type = CALENDAR_TYPE.Work;
                        } else
                        {
                            // 上班日期里再根据请假，考勤判断是：请假，迟到，早退，正常上班
                            var attence = attenceRecords.Where(n => n.StartTime >= d.Date && n.EndTime <= CommonUtils.EndOfDay(d)).FirstOrDefault();
                            if (attence == null)
                            {
                                // 判断是否请假
                                var leave = leaveRecord.Where(n => n.StartDate >= d.Date || n.EndDate <= CommonUtils.EndOfDay(d)).FirstOrDefault();
                                if (leave == null)
                                {
                                    // 判断是否有出差
                                    var bizTrip = bizTripRecord.Where(n => n.StartDate >= d.Date || n.EndDate <= CommonUtils.EndOfDay(d)).FirstOrDefault();
                                    if (bizTrip != null)
                                    {
                                        entity.Name = "出差";
                                        entity.Type = CALENDAR_TYPE.BizTrp;
                                    }
                                    else
                                    {
                                        // 没有请假，没有出差，没有打卡，就是旷工
                                        entity.Name = "旷工";
                                        entity.Type = CALENDAR_TYPE.Absent;
                                    }
                                }
                                else
                                {
                                    entity.Name = "请假";
                                    entity.Type = CALENDAR_TYPE.Leave;
                                }
                            }
                            else
                            {
                                // 判断考勤状态
                                var shouldStartTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.StartWorkTime)));
                                var shouldEndTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:59", sysConfig.EndWorkTime)));
                                if (attence.StartTime > shouldStartTime)
                                {
                                    entity.Name = "迟到";
                                    entity.Type = CALENDAR_TYPE.Late;
                                }
                                else if (attence.EndTime < shouldEndTime)
                                {
                                    entity.Name = "早退";
                                    entity.Type = CALENDAR_TYPE.LeaveEarly;
                                }
                                else
                                {
                                    entity.Name = "上班";
                                    entity.Type = CALENDAR_TYPE.Work;
                                }
                            }
                        }
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