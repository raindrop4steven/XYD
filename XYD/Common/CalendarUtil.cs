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

        //#region 统计用户考勤
        //public static XYD_Calendar_Result CaculateUserCalendar(Employee employee, DateTime BeginDate, DateTime EndDate)
        //{
        //    var calendarResult = new XYD_Calendar_Result();
        //    var db = new DefaultConnection();

        //    List<XYD_CalendarEntity> dates = new List<XYD_CalendarEntity>();
        //    Dictionary<CALENDAR_TYPE, int> summary = new Dictionary<CALENDAR_TYPE, int>() {
        //            {CALENDAR_TYPE.Holiday, 0 },
        //            {CALENDAR_TYPE.Adjust, 0 },
        //            {CALENDAR_TYPE.Rest, 0 },
        //            {CALENDAR_TYPE.Work, 0 },
        //            {CALENDAR_TYPE.Late, 0 },
        //            {CALENDAR_TYPE.LeaveEarly, 0 },
        //            {CALENDAR_TYPE.Absent, 0 },
        //            {CALENDAR_TYPE.Leave, 0 },
        //            {CALENDAR_TYPE.BizTrp, 0 }
        //        };
        //    // 获得指定年份放假和调休计划
        //    var StartDate = BeginDate.Date;

        //    int currentYear = BeginDate.Year;
        //    var calendar = GetPlanByYear(currentYear);
        //    var holidayDict = GetHolidays(calendar);
        //    var adjustDict = GetAdjusts(calendar);
        //    // 获得考勤记录
        //    var lastDayTime = CommonUtils.EndOfDay(EndDate);
        //    var attenceRecords = db.Attence.Where(n => n.EmplNo == employee.EmplNO && n.StartTime >= StartDate.Date && n.EndTime <= lastDayTime).OrderBy(n => n.StartTime).ToList();
        //    // 获得请假记录
        //    var leaveRecord = db.LeaveRecord.Where(n => n.EmplID == employee.EmplID && n.StartDate >= StartDate.Date && n.EndDate <= lastDayTime).OrderBy(n => n.StartDate).ToList();
        //    // 获得出差记录
        //    var bizTripRecord = db.BizTrip.Where(n => n.EmplID == employee.EmplID && n.StartDate >= StartDate.Date && n.EndDate <= lastDayTime).OrderBy(n => n.StartDate).ToList();
        //    // 获得对应城市工作时间配置
        //    var workArea = OrgUtil.GetWorkArea(employee.EmplID);
        //    var sysConfig = db.SystemConfig.Where(n => n.Area == workArea).FirstOrDefault();
        //    // 今天
        //    var today = DateTime.Now.Date;
        //    // 判断每一天状态
        //    for (DateTime d = StartDate; d <= EndDate; d = d.AddDays(1))
        //    {
        //        var entity = new XYD_CalendarEntity();
        //        var detail = new XYD_CalendarDetail();
        //        CalendarCaculate(d, today, holidayDict, db, sysConfig, attenceRecords, leaveRecord, bizTripRecord, ref entity, ref detail);
        //        summary[entity.Type] += 1;
        //        dates.Add(entity);
        //    }
        //    calendarResult.summary = summary;
        //    calendarResult.dates = dates;
        //    return calendarResult;
        //}
        //#endregion

        #region 考勤详情
        public static XYD_Calendar_Result CaculateUserCalendarDetail(Employee employee, DateTime BeginDate, DateTime EndDate)
        {
            var calendarResult = new XYD_Calendar_Result();
            List<XYD_CalendarEntity> dates = new List<XYD_CalendarEntity>();
            var db = new DefaultConnection();
            var StartDate = BeginDate.Date;
            var lastDayTime = CommonUtils.EndOfDay(EndDate);
            var today = DateTime.Now.Date;
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
            // 获得本年放假和调休计划
            int currentYear = BeginDate.Year;
            var calendar = GetPlanByYear(currentYear);
            var holidayDict = GetHolidays(calendar);
            var adjustDict = GetAdjusts(calendar);
            // 获得考勤记录
            var attenceRecords = db.Attence.Where(n => n.EmplNo == employee.EmplNO && n.StartTime >= StartDate.Date && n.EndTime <= lastDayTime).OrderBy(n => n.StartTime).ToList();
            var leaveRecord = db.LeaveRecord.Where(n => n.EmplID == employee.EmplID && n.StartDate >= StartDate.Date && n.EndDate <= lastDayTime).OrderBy(n => n.StartDate).ToList();
            var bizTripRecord = db.BizTrip.Where(n => n.EmplID == employee.EmplID && n.StartDate >= StartDate.Date && n.EndDate <= lastDayTime).OrderBy(n => n.StartDate).ToList();
            // 获得对应城市工作时间配置
            var workArea = OrgUtil.GetWorkArea(employee.EmplID);
            var sysConfig = db.SystemConfig.Where(n => n.Area == workArea).FirstOrDefault();
            // 判断每一天状态
            for (DateTime d = StartDate; d <= EndDate; d = d.AddDays(1))
            {
                var entity = new XYD_CalendarEntity();
                var detail = new XYD_CalendarDetail();
                CalendarCaculate(d, today, holidayDict, db, sysConfig, attenceRecords, leaveRecord, bizTripRecord, ref entity, ref detail);
                summary[entity.Type] += 1;
                dates.Add(entity);
                details.Add(detail);
            }
            calendarResult.summary = summary;
            calendarResult.details = details;
            calendarResult.dates = dates;
            return calendarResult;
        }
        #endregion

        #region 判断用户当日考勤
        public static void CalendarCaculate(DateTime d, DateTime today, Dictionary<string, string> holidayDict, DefaultConnection db, XYD_System_Config sysConfig, List<XYD_Attence> attenceRecords, List<XYD_Leave_Record> leaveRecord, List<XYD_BizTrip> bizTripRecord, ref XYD_CalendarEntity entity, ref XYD_CalendarDetail detail)
        {
            var date = d.ToString("yyyy-MM-dd");
            //var entity = new XYD_CalendarEntity();
            //var detail = new XYD_CalendarDetail();
            entity.Date = date;
            detail.Date = date;
            // 判断考勤状态
            var shouldStartTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.StartWorkTime)));
            var shouldEndTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:59", sysConfig.EndWorkTime)));
            var restStartTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.RestStartTime)));
            var restEndTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.RestEndTime)));
            // 考勤记录
            var attence = attenceRecords.Where(n => n.StartTime >= d.Date && n.EndTime <= CommonUtils.EndOfDay(d)).FirstOrDefault();
            // 出勤记录
            var leave = leaveRecord.Where(n => n.StartDate >= d.Date && n.EndDate <= CommonUtils.EndOfDay(d) && n.Status == Leave_Status_YES).FirstOrDefault();
            // 出差记录
            var bizTrip = bizTripRecord.Where(n => n.StartDate >= d.Date && n.EndDate <= CommonUtils.EndOfDay(d)).FirstOrDefault();

            // 定义对应考勤请假出差变量
            var hasAttence = attence != null;
            var hasLeave = leave != null;
            var hasBizTrip = bizTrip != null;
            // 工作时长
            var workHours = CaculateWorkHours(d, attence, sysConfig);
            /**
             * 判断逻辑：
             * 1. 是否该休息
             * 2. 是否该上班
             */
            // 当天是否该休息
            if (holidayDict.ContainsKey(date) || d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
            {
                if (hasAttence)
                {
                    entity.Name = "上班";
                    entity.Type = CALENDAR_TYPE.Work;
                }
                else if (holidayDict.ContainsKey(date))
                {
                    entity.Name = holidayDict[date];
                    entity.Type = CALENDAR_TYPE.Holiday;
                }
                else
                {
                    entity.Name = "休息";
                    entity.Type = CALENDAR_TYPE.Rest;
                }
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
                    if (!hasAttence)
                    {
                        // 判断是否请假
                        if (hasLeave)
                        {
                            var leaveHour = Math.Round(leave.EndDate.Subtract(leave.StartDate).TotalHours, 2);
                            entity.Name = leave.Category;
                            // 是否是请假，补外勤，补打卡，加班，外勤
                            if (IsLeaveAsWork(leave))
                            {
                                entity.Type = CALENDAR_TYPE.Work;
                            }
                            else
                            {
                                entity.Type = CALENDAR_TYPE.Leave;
                            }
                            // 增加备注
                            detail.Memo += string.Format("今日{0}{1}小时", leave.Category, leaveHour);
                        }
                        else
                        {
                            // 判断是否有出差
                            if (hasBizTrip)
                            {
                                entity.Name = "出差";
                                entity.Type = CALENDAR_TYPE.BizTrp;
                                // 记录详情
                                var serialRecord = db.SerialRecord.Where(n => n.MessageID == bizTrip.MessageID).FirstOrDefault();
                                detail.Memo += string.Format("今日出差，出差编号:{0}", serialRecord == null ? serialRecord.Sn : string.Empty);
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
                        // 有考勤，又请假，显示请假信息即可
                        if (hasLeave)
                        {
                            var leaveHour = Math.Round(leave.EndDate.Subtract(leave.StartDate).TotalHours, 2);
                            detail.Memo += string.Format("今日{0}{1}小时", leave.Category, leaveHour);
                        }
                        if (hasBizTrip)
                        {
                            var serialRecord = db.SerialRecord.Where(n => n.MessageID == bizTrip.MessageID).FirstOrDefault();
                            detail.Memo += string.Format("今日出差，出差编号:{0}", serialRecord == null ? serialRecord.Sn : string.Empty);
                        }
                        // 加入午休时间逻辑
                        if (attence.StartTime > shouldStartTime)
                        {
                            if ((hasLeave || hasBizTrip) && workHours >= 8)
                            {
                                // 早上请假，然后打卡上班，总时长超过8小时
                                entity.Name = "上班";
                                entity.Type = CALENDAR_TYPE.Work;
                            }
                            else
                            {
                                entity.Name = "迟到";
                                entity.Type = CALENDAR_TYPE.Late;
                            }
                        }
                        else
                        {
                            // 如果是今天，则不判断早退
                            if (d == today || workHours >= 8)
                            {
                                entity.Name = "上班";
                                entity.Type = CALENDAR_TYPE.Work;
                            }
                            else
                            {
                                entity.Name = "早退";
                                entity.Type = CALENDAR_TYPE.LeaveEarly;
                            }
                        }
                    }
                }
            }

            // 记录详情
            if (hasAttence)
            {
                detail.StartTime = attence.StartTime == null ? "" : attence.StartTime.Value.ToString("yyyy-MM-dd HH:mm");
                detail.EndTime = attence.EndTime == null ? "" : attence.EndTime.Value.ToString("yyyy-MM-dd HH:mm");
            }
            detail.WorkHours = workHours;
            detail.Name = entity.Name;
            detail.Type = entity.Type;
        }
        #endregion

        #region 判断出勤是否该算入工作时间
        public static bool IsLeaveAsWork(XYD_Leave_Record leave)
        {
            if (leave.Category.Contains("假") || leave.Category.Contains("补") || leave.Category == "加班" || leave.Category == "外勤")
            {
                return true;
            }
            return false;
        }
        #endregion

        #region 计算每日工作小时数,排除午休时间
        public static double CaculateWorkHours(DateTime d, XYD_Attence attence, XYD_System_Config sysConfig)
        {
            double workHours = 0;
            var restStartTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.RestStartTime)));
            var restEndTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.RestEndTime)));

            // 今日无考勤或首次打卡没有
            if (attence == null || attence.StartTime == null)
            {
                return workHours;
            }
            // 没有末次打卡
            if (attence.EndTime == null)
            {
                if (d.Date == DateTime.Now.Date) // 如果是今天，则按照工作时间来计算
                {
                    attence.EndTime = DateTime.Now;
                }
                else
                {
                    return workHours;
                }
            }
            // 计算工作时长
            if (attence.EndTime <= restStartTime || attence.StartTime >= restEndTime)
            {
                workHours = attence.EndTime.Value.Subtract(attence.StartTime.Value).TotalHours;
            } else if (attence.StartTime < restStartTime && attence.EndTime > restStartTime && attence.EndTime < restEndTime)
            {
                workHours = restStartTime.Subtract(attence.StartTime.Value).TotalHours;
            } else if (attence.StartTime > restStartTime && attence.StartTime < restEndTime && attence.EndTime > restEndTime)
            {
                workHours = attence.EndTime.Value.Subtract(restEndTime).TotalHours;
            } else if (attence.StartTime < restStartTime && attence.EndTime > restEndTime)
            {
                workHours = restStartTime.Subtract(attence.StartTime.Value).TotalHours + attence.EndTime.Value.Subtract(restEndTime).TotalHours;
            } else if (attence.StartTime >= restStartTime && attence.EndTime <= restEndTime)
            {
                return workHours;
            }

            return Math.Round(workHours, 2);
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
            if (userCompanyInfo == null)
            {
                return 0;
            }
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

        #region 更新考勤记录
        public static void UpdateAttence(Employee employee, XYD_Leave_Record leave)
        {
            using (var db = new DefaultConnection())
            {
                var startTime = leave.StartDate.Date;
                var endTime = CommonUtils.EndOfDay(leave.EndDate);
                var attence = db.Attence.Where(n => n.EmplNo == employee.EmplNO && n.StartTime >= startTime && n.EndTime <= endTime).FirstOrDefault();
                if (attence == null)
                {
                    attence = new XYD_Attence();
                    attence.EmplNo = employee.EmplNO;
                    attence.EmplName = employee.EmplName;
                    attence.StartTime = leave.StartDate;
                    attence.EndTime = leave.EndDate;
                    attence.Day = leave.StartDate.ToString("yyyyMMdd");
                    attence.DeviceID = "新友达";
                    db.Attence.Add(attence);
                } else
                {
                    // 更新，最早考勤取最早，最晚考勤取最晚
                    if (attence.StartTime.Value > leave.StartDate)
                    {
                        attence.StartTime = leave.StartDate;
                    }
                    if (attence.EndTime == null || (attence.EndTime != null && attence.EndTime < leave.EndDate))
                    {
                        attence.EndTime = leave.EndDate;
                    }
                }
                db.SaveChanges();
            }
        }
        #endregion
    }
}