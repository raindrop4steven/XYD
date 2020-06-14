﻿using Appkiz.Library.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using XYD.Entity;
using XYD.Models;
using static XYD.Common.DEP_Constants;

namespace XYD.Common
{
    public class CalendarUtil
    {
        #region 获得指定年份休假和调休安排
        public static XYD_Calendar GetPlanByYear(int currentYear)
        {
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], "holidays", string.Format("{0}.json", currentYear));

            using (StreamReader sr = new StreamReader(filePathName))
            {
                var calendar = JsonConvert.DeserializeObject<XYD_Calendar>(sr.ReadToEnd());
                return calendar;
            }
        }
        #endregion

        #region 获取节假日
        public static Dictionary<string, string> GetHolidays(XYD_Calendar calendar)
        {
            var dict = new Dictionary<string, string>();
            var holidays = calendar.holidays;
            foreach (var item in holidays)
            {
                foreach(var day in item.Days)
                {
                    dict.Add(day, item.Name);
                }
            }
            return dict;
        }
        #endregion

        #region 获取调休
        public static Dictionary<string, string> GetAdjusts(XYD_Calendar calendar)
        {
            var dict = new Dictionary<string, string>();
            var adjust = calendar.adjust;
            foreach (var item in adjust)
            {
                foreach (var day in item.Days)
                {
                    dict.Add(day, item.Name);
                }
            }
            return dict;
        }
        #endregion

        #region 计算总应该上班天数
        public static int CaculateShouldWorkDays(DateTime beginDate, DateTime endDate)
        {
            // 总天数
            int totoalDays = 0;
            // 获得指定年份放假和调休计划
            var StartDate = beginDate.Date;
            int currentYear = beginDate.Year;
            var calendar = CalendarUtil.GetPlanByYear(currentYear);
            var holidayDict = CalendarUtil.GetHolidays(calendar);
            var adjustDict = CalendarUtil.GetAdjusts(calendar);
            // 判断每一天状态
            for (DateTime d = StartDate; d <= endDate; d = d.AddDays(1))
            {
                var date = d.ToString("yyyy-MM-dd");
                var entity = new XYD_CalendarEntity();
                entity.Date = date;
                // 首先判断是否是节日
                if (holidayDict.ContainsKey(date) || adjustDict.ContainsKey(date) || d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }
                totoalDays += 1;
            }
            return totoalDays;
        }
        #endregion

        #region 统计用户考勤
        public static XYD_Calendar_Result CaculateUserCalendar(Employee employee, DateTime BeginDate, DateTime EndDate)
        {
            var calendarResult = new XYD_Calendar_Result();
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
            var StartDate = BeginDate.Date;
            //EndDate = CommonUtils.EndOfDay(EndDate.AddMonths(1).AddDays(-1));

            int currentYear = BeginDate.Year;
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
            for (DateTime d = StartDate; d <= EndDate; d = d.AddDays(1))
            {
                var date = d.ToString("yyyy-MM-dd");
                var entity = new XYD_CalendarEntity();
                entity.Date = date;
                // 首先判断是否是节日
                if (holidayDict.ContainsKey(date))
                {
                    entity.Name = holidayDict[date];
                    entity.Type = CALENDAR_TYPE.Holiday;
                }
                else if (adjustDict.ContainsKey(date))
                {
                    entity.Name = adjustDict[date];
                    entity.Type = CALENDAR_TYPE.Adjust;
                }
                else if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                {
                    entity.Name = "休息";
                    entity.Type = CALENDAR_TYPE.Rest;
                }
                else
                {
                    // 超过今天的，不用做判断，直接上班
                    if (d > today)
                    {
                        entity.Name = "上班";
                        entity.Type = CALENDAR_TYPE.Work;
                    }
                    // 今天之前的数据
                    else
                    {
                        // 上班日期里再根据请假，考勤判断是：请假，迟到，早退，正常上班
                        var attence = attenceRecords.Where(n => n.StartTime >= d.Date && n.EndTime <= CommonUtils.EndOfDay(d)).FirstOrDefault();
                        if (attence == null)
                        {
                            // 判断是否请假
                            var leave = leaveRecord.Where(n => n.StartDate >= d.Date && n.EndDate <= CommonUtils.EndOfDay(d)).FirstOrDefault();
                            if (leave != null)
                            {
                                if (leave.Category != null && leave.Category.Contains("假"))
                                {
                                    entity.Name = "请假";
                                    entity.Type = CALENDAR_TYPE.Leave;
                                }
                                else
                                {
                                    entity.Name = "上班";
                                    entity.Type = CALENDAR_TYPE.Work;
                                }
                            }
                            else
                            {
                                // 判断是否有出差
                                var bizTrip = bizTripRecord.Where(n => n.StartDate >= d.Date && n.EndDate <= CommonUtils.EndOfDay(d)).FirstOrDefault();
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
                        }
                        else
                        {
                            // 判断考勤状态
                            var shouldStartTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.StartWorkTime)));
                            var shouldEndTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:59", sysConfig.EndWorkTime)));
                            var restStartTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.RestStartTime)));
                            var restEndTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.RestEndTime)));
                            // 加入午休时间逻辑
                            if (attence.StartTime > shouldStartTime)
                            {
                                entity.Name = "迟到";
                                entity.Type = CALENDAR_TYPE.Late;
                            }
                            else
                            {
                                // 如果是今天，则不判断早退
                                if (d == today || attence.EndTime == null)
                                {
                                    entity.Name = "上班";
                                    entity.Type = CALENDAR_TYPE.Work;
                                }
                                else
                                {
                                    var morningHour = (restStartTime - attence.StartTime.Value).TotalHours;
                                    var afterHour = (attence.EndTime.Value - restEndTime).TotalHours;
                                    var workHour = morningHour + afterHour;
                                    if (workHour < 8)
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
                    }
                }
                summary[entity.Type] += 1;
                dates.Add(entity);
            }
            calendarResult.summary = summary;
            calendarResult.dates = dates;
            return calendarResult;
        }
        #endregion

        #region 考勤详情
        public static XYD_Calendar_Result CaculateUserCalendarDetail(Employee employee, DateTime BeginDate, DateTime EndDate)
        {
            var calendarResult = new XYD_Calendar_Result();
            var db = new DefaultConnection();

            List<XYD_CalendarDetail> details = new List<XYD_CalendarDetail>();
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
            var StartDate = BeginDate.Date;
            //EndDate = CommonUtils.EndOfDay(EndDate.AddMonths(1).AddDays(-1));

            int currentYear = BeginDate.Year;
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
            for (DateTime d = StartDate; d <= EndDate; d = d.AddDays(1))
            {
                var date = d.ToString("yyyy-MM-dd");
                var entity = new XYD_CalendarEntity();
                var detail = new XYD_CalendarDetail();
                entity.Date = date;
                detail.Date = date;
                // 首先判断是否是节日
                if (holidayDict.ContainsKey(date))
                {
                    entity.Name = holidayDict[date];
                    entity.Type = CALENDAR_TYPE.Holiday;
                }
                else if (adjustDict.ContainsKey(date))
                {
                    entity.Name = adjustDict[date];
                    entity.Type = CALENDAR_TYPE.Adjust;
                }
                else if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                {
                    entity.Name = "休息";
                    entity.Type = CALENDAR_TYPE.Rest;
                }
                else
                {
                    // 超过今天的，不用做判断，直接上班
                    if (d > today)
                    {
                        entity.Name = "上班";
                        entity.Type = CALENDAR_TYPE.Work;
                    }
                    // 今天之前的数据
                    else
                    {
                        // 上班日期里再根据请假，考勤判断是：请假，迟到，早退，正常上班
                        var attence = attenceRecords.Where(n => n.StartTime >= d.Date && n.EndTime <= CommonUtils.EndOfDay(d)).FirstOrDefault();
                        if (attence == null)
                        {
                            // 判断是否请假
                            var leave = leaveRecord.Where(n => n.StartDate >= d.Date && n.EndDate <= CommonUtils.EndOfDay(d)).FirstOrDefault();
                            if (leave != null)
                            {
                                if (leave.Category != null && leave.Category.Contains("假"))
                                {
                                    entity.Name = "请假";
                                    entity.Type = CALENDAR_TYPE.Leave;
                                }
                                else
                                {
                                    entity.Name = "上班";
                                    entity.Type = CALENDAR_TYPE.Work;
                                }
                                // 记录详情
                                detail.StartTime = leave.StartDate.ToString("yyyy-MM-dd HH:mm");
                                detail.EndTime = leave.EndDate.ToString("yyyy-MM-dd HH:mm");
                            }
                            else
                            {
                                // 判断是否有出差
                                var bizTrip = bizTripRecord.Where(n => n.StartDate >= d.Date && n.EndDate <= CommonUtils.EndOfDay(d)).FirstOrDefault();
                                if (bizTrip != null)
                                {
                                    entity.Name = "出差";
                                    entity.Type = CALENDAR_TYPE.BizTrp;
                                    // 记录详情
                                    detail.StartTime = bizTrip.StartDate.ToString("yyyy-MM-dd HH:mm");
                                    detail.EndTime = bizTrip.EndDate.ToString("yyyy-MM-dd HH:mm");
                                }
                                else
                                {
                                    // 没有请假，没有出差，没有打卡，就是旷工
                                    entity.Name = "旷工";
                                    entity.Type = CALENDAR_TYPE.Absent;
                                }
                            }
                        }
                        else
                        {
                            // 判断考勤状态
                            var shouldStartTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.StartWorkTime)));
                            var shouldEndTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:59", sysConfig.EndWorkTime)));
                            var restStartTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.RestStartTime)));
                            var restEndTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.RestEndTime)));
                            // 加入午休时间逻辑
                            if (attence.StartTime > shouldStartTime)
                            {
                                entity.Name = "迟到";
                                entity.Type = CALENDAR_TYPE.Late;
                            }
                            else
                            {
                                // 如果是今天，则不判断早退
                                if (d == today || attence.EndTime == null)
                                {
                                    entity.Name = "上班";
                                    entity.Type = CALENDAR_TYPE.Work;
                                }
                                else
                                {
                                    var morningHour = (restStartTime - attence.StartTime.Value).TotalHours;
                                    var afterHour = (attence.EndTime.Value - restEndTime).TotalHours;
                                    var workHour = morningHour + afterHour;
                                    if (workHour < 8)
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
                            // 记录详情
                            detail.StartTime = attence.StartTime == null ? "" : attence.StartTime.Value.ToString("yyyy-MM-dd HH:mm");
                            detail.EndTime = attence.EndTime == null ? "" : attence.EndTime.Value.ToString("yyyy-MM-dd HH:mm");
                        }
                    }
                }
                detail.Name = entity.Name;
                detail.Type = entity.Type;
                summary[entity.Type] += 1;
                details.Add(detail);
            }
            calendarResult.summary = summary;
            calendarResult.details = details;
            return calendarResult;
        }
        #endregion

        #region 计算年假
        /// <summary>
        /// * 根据实际工龄计算年假
                /// * 实际工龄不满十年，年假5天
        /// * 满十年不满二十年，年假10天
        /// * 满20年，年假15天
        /// </summary>
        /// <param name="SocialMonth"></param>
        /// <returns></returns>
        public static int CaculateYearRestDays(XYD_UserCompanyInfo userCompanyInfo)
        {
            if (userCompanyInfo.ManualCaculate)
            {
                return userCompanyInfo.RestDays;
            }
            else
            {
                if (userCompanyInfo.SocialInsuranceTotalMonth < 10 * 12)
                {
                    return 5;
                }
                else if (userCompanyInfo.SocialInsuranceTotalMonth < 20 * 12)
                {
                    return 10;
                }
                else
                {
                    return 15;
                }
            }
        }
        #endregion

        #region 计算实际请假时间
        public static double GetRealLeaveHours(string EmplID, DateTime startTime, DateTime endTime)
        {
            using (var db = new DefaultConnection())
            {
                var workArea = OrgUtil.GetWorkArea(EmplID);
                var sysConfig = db.SystemConfig.Where(n => n.Area == workArea).FirstOrDefault();
                var days = endTime.Subtract(startTime).TotalDays;
                var subSum = 0.0;

                if (days == 0)
                {
                    var restStartTime = DateTime.Parse(startTime.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.RestStartTime)));
                    var restEndTime = DateTime.Parse(startTime.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.RestEndTime)));
                    if (startTime < restStartTime)
                    {
                        subSum += restStartTime.Subtract(startTime).TotalHours;
                    }
                    if (endTime > restEndTime)
                    {
                        subSum += endTime.Subtract(restEndTime).TotalHours;
                    }
                }
                else
                {
                    subSum = endTime.Subtract(startTime).TotalDays * 8;
                }
                return subSum;
            }
        }
        #endregion
    }
}