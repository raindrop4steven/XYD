using Appkiz.Library.Security;
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
                if (holidayDict.ContainsKey(date) || d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }
                totoalDays += 1;
            }
            return totoalDays;
        }
        #endregion

        #region 考勤详情
        public static XYD_Calendar_Result CaculateUserCalendarDetail(Employee employee, DateTime BeginDate, DateTime EndDate, bool needLeave=false)
        {
            var calendarResult = new XYD_Calendar_Result();
            List<XYD_CalendarEntity> dates = new List<XYD_CalendarEntity>();
            var db = new DefaultConnection();
            var StartDate = BeginDate.Date;
            var lastDayTime = CommonUtils.EndOfDay(EndDate);
            var today = DateTime.Now.Date;
            var sameDay = BeginDate.Date == EndDate.Date;
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
            //var attenceRecords = db.Attence.Where(n => n.EmplNo == employee.EmplNO && n.StartTime >= StartDate.Date && (n.EndTime <= lastDayTime || n.EndTime == null)).OrderBy(n => n.StartTime).ToList();
            var attenceRecords = db.Attence.Where(n => n.EmplNo == employee.EmplNO).ToList().Where(n => DateTime.Parse(n.Day) >= StartDate.Date && DateTime.Parse(n.Day) <= lastDayTime).OrderBy(n => n.StartTime).ToList();
            // 区分按人和按日查询的条件，如果按照人查询，则条件开始时间<出勤开始 && 出勤结束 <条件结束;如果按照日查询，则出勤时间区间应包括条件时间
            var leaveRecord = db.LeaveRecord.Where(n => n.EmplID == employee.EmplID && ((n.StartDate <= StartDate.Date && n.EndDate >= EndDate.Date) || (n.StartDate >= StartDate.Date && n.EndDate <= lastDayTime))).ToList();
            var bizTripRecord = db.BizTrip.Where(n => n.EmplID == employee.EmplID && n.StartDate <= lastDayTime && n.EndDate >= StartDate.Date).ToList();
            
            // 获得对应城市工作时间配置
            var sysConfig = GetSysConfigByUser(employee.EmplID);
            // 判断每一天状态
            for (DateTime d = StartDate; d <= EndDate; d = d.AddDays(1))
            {
                var entity = new XYD_CalendarEntity();
                var detail = new XYD_CalendarDetail();
                CalendarCaculate(d, today, holidayDict, db, sysConfig, attenceRecords, leaveRecord, bizTripRecord, ref entity, ref detail);
                
                if (detail.isNormal && entity.Type != CALENDAR_TYPE.Rest && entity.Type != CALENDAR_TYPE.Holiday && entity.Type != CALENDAR_TYPE.Work)
                {
                    summary[CALENDAR_TYPE.Work] += 1;
                }
                else
                {
                    summary[entity.Type] += 1;
                }
                dates.Add(entity);
                details.Add(detail);
            }
            
            // 如果是app中请假则需要统计进去
            if (needLeave)
            {
                // 替换对应请假出差记录
                // 调休
                var adjustLeaveList = leaveRecord.Where(n => n.Category == "调休").ToList();
                // 请假
                var normalLeaveList = leaveRecord.Where(n => n.Category.Contains("假")).ToList();
                summary[CALENDAR_TYPE.Adjust] = adjustLeaveList.Count;
                summary[CALENDAR_TYPE.Leave] = normalLeaveList.Count;
                summary[CALENDAR_TYPE.BizTrp] = bizTripRecord.Count;
                // 转化成考情详情
                foreach(var leave in adjustLeaveList)
                {
                    var detailEntity = CreateLeaveCalendarDetail(sysConfig, leave);
                    details.Add(detailEntity);
                }
                // 转化成考情详情
                foreach (var leave in normalLeaveList)
                {
                    var detailEntity = CreateLeaveCalendarDetail(sysConfig, leave);
                    details.Add(detailEntity);
                }
                // 转化成考情详情
                foreach (var trip in bizTripRecord)
                {
                    var detailEntity = CreateTripCalendarDetail(sysConfig, trip);
                    details.Add(detailEntity);
                }
                details = details.Where(n => !(n.Type != CALENDAR_TYPE.Absent && n.StartTime == null)).ToList();
            }
            calendarResult.summary = summary;
            calendarResult.details = details;
            calendarResult.dates = dates;
            return calendarResult;
        }
        #endregion

        #region 创建考勤详情
        public static XYD_CalendarDetail CreateLeaveCalendarDetail(XYD_System_Config sysConfig, XYD_Leave_Record leave)
        {
            var detailEntity = new XYD_CalendarDetail();
            if (IsDayDate(leave.StartDate))
            {
                detailEntity.StartTime = leave.StartDate.ToString("yyyy-MM-dd");
                detailEntity.EndTime = leave.EndDate.ToString("yyyy-MM-dd");
                detailEntity.WorkHours = (int)(GetRealLeaveHours(sysConfig, leave.StartDate, leave.EndDate) / 8);
            }
            else
            {
                detailEntity.StartTime = leave.StartDate.ToString("yyyy-MM-dd HH:mm");
                detailEntity.EndTime = leave.EndDate.ToString("yyyy-MM-dd HH:mm");
                detailEntity.WorkHours = GetRealLeaveHours(sysConfig, leave.StartDate, leave.EndDate);
            }
            detailEntity.Date = leave.StartDate.ToString("yyyy-MM-dd");
            detailEntity.Name = leave.Category;
            if (leave.Category == "调休")
            {
                detailEntity.Type = CALENDAR_TYPE.Adjust;
            }
            else
            {
                detailEntity.Type = CALENDAR_TYPE.Leave;
            }
            
            return detailEntity;
        }
        #endregion

        #region 创建出差考勤详情
        public static XYD_CalendarDetail CreateTripCalendarDetail(XYD_System_Config sysConfig, XYD_BizTrip trip)
        {
            var detailEntity = new XYD_CalendarDetail();
            if (IsDayDate(trip.StartDate))
            {
                detailEntity.StartTime = trip.StartDate.ToString("yyyy-MM-dd");
                detailEntity.EndTime = trip.EndDate.ToString("yyyy-MM-dd");
                detailEntity.WorkHours = (int)(GetRealLeaveHours(sysConfig, trip.StartDate, trip.EndDate) / 8);
            }
            else
            {
                detailEntity.StartTime = trip.StartDate.ToString("HH:mm");
                detailEntity.EndTime = trip.EndDate.ToString("HH:mm");
                detailEntity.WorkHours = GetRealLeaveHours(sysConfig, trip.StartDate, trip.EndDate);
            }
            detailEntity.Date = trip.StartDate.ToString("yyyy-MM-dd");
            detailEntity.Name = "出差";
            detailEntity.Type = CALENDAR_TYPE.BizTrp;
            return detailEntity;
        }
        #endregion

        #region 判断用户当日考勤
        public static void CalendarCaculate(DateTime d, DateTime today, Dictionary<string, string> holidayDict, DefaultConnection db, XYD_System_Config sysConfig, List<XYD_Attence> attenceRecords, List<XYD_Leave_Record> leaveRecord, List<XYD_BizTrip> bizTripRecord, ref XYD_CalendarEntity entity, ref XYD_CalendarDetail detail)
        {
            var date = d.ToString("yyyy-MM-dd");
            var lastDayTime = CommonUtils.EndOfDay(d);
            entity.Date = date;
            detail.Date = date;
            // 判断考勤状态
            var shouldStartTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.StartWorkTime)));
            var shouldEndTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:59", sysConfig.EndWorkTime)));
            var restStartTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.RestStartTime)));
            var restEndTime = DateTime.Parse(d.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.RestEndTime)));
            // 考勤记录
            //var attence = attenceRecords.Where(n => n.StartTime >= d.Date && (n.EndTime <= lastDayTime || n.EndTime == null)).FirstOrDefault();
            var attence = attenceRecords.Where(n => DateTime.Parse(n.Day) == d.Date).FirstOrDefault();
            // 出勤记录: 一种是出勤时间在一天内，属于小时，为条件1；第二种隔天是天数，为条件2
            var leave = leaveRecord.Where(n => ((n.StartDate >= d.Date && n.EndDate <= lastDayTime) || (n.StartDate <= d.Date && CommonUtils.EndOfDay(n.EndDate) >= d.Date)) && n.Status == Leave_Status_YES).FirstOrDefault();
            // 出差记录
            var bizTrip = bizTripRecord.Where(n => ((n.StartDate >= d.Date && n.EndDate <= lastDayTime) || (n.StartDate <= d.Date && CommonUtils.EndOfDay(n.EndDate) >= d.Date))).FirstOrDefault();

            // 定义对应考勤请假出差变量
            var hasAttence = attence != null;
            var hasLeave = leave != null;
            var hasBizTrip = bizTrip != null;
            // 工作考勤时长
            var workHours = CaculateWorkHours(d, attence, sysConfig);
            var leaveHour = 0.0d;
            /**
             * 判断逻辑：
             * 1. 是否该休息
             * 2. 是否该上班
             */
            // 当天是否该休息
            if (holidayDict.ContainsKey(date) || d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
            {
                var isHoliday = holidayDict.ContainsKey(date);
                if (hasAttence)
                {
                    entity.Name = "上班";
                    entity.Type = CALENDAR_TYPE.Work;
                }
                if (hasLeave && !NeedUpdateAttence(leave))
                {
                    leaveHour = GetRealLeaveHours(sysConfig, leave.StartDate, leave.EndDate);
                    if (workHours == 0)
                    {
                        workHours = leaveHour;
                    }
                    entity.Type = isHoliday ? CALENDAR_TYPE.Holiday : CALENDAR_TYPE.Rest;
                    entity.Name = leave.Category;
                    // 增加备注
                    detail.Memo += GenerateLeaveMemo(leave, leaveHour);
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
                            leaveHour = GetRealLeaveHours(sysConfig, leave.StartDate, leave.EndDate);
                            entity.Name = leave.Category;
                            // 是否是补外勤，补打卡，加班，外勤
                            if (!NeedUpdateAttence(leave))
                            {
                                if(IsLeaveAsWork(leave))
                                {
                                    if (IsDayDate(leave.StartDate) && IsDayDate(leave.EndDate))
                                    {
                                        leaveHour = Normal_Work_Hours;
                                    }
                                    workHours += leaveHour;
                                    entity.Type = CALENDAR_TYPE.Work;
                                }
                                else
                                {
                                    entity.Type = CALENDAR_TYPE.Leave;
                                    if (IsDayDate(leave.StartDate) && IsDayDate(leave.EndDate))
                                    {
                                        leaveHour = Normal_Work_Hours;
                                    }
                                }
                            }
                            
                            // 增加备注
                            detail.Memo += GenerateLeaveMemo(leave, leaveHour);
                        }
                        else
                        {
                            // 判断是否有出差
                            if (hasBizTrip)
                            {
                                entity.Name = "出差";
                                entity.Type = CALENDAR_TYPE.BizTrp;
                                workHours = Normal_Work_Hours;
                                // 记录详情
                                var serialRecord = db.SerialRecord.Where(n => n.MessageID == bizTrip.MessageID).FirstOrDefault();
                                detail.Memo += string.Format("今日出差，出差编号:{0}", serialRecord == null ? string.Empty : serialRecord.Sn);
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
                        // 有考勤，又请假，说明是小时假
                        if (hasLeave)
                        {
                            leaveHour =  GetRealLeaveHours(sysConfig, leave.StartDate, leave.EndDate);
                            if(IsLeaveAsWork(leave) && !NeedUpdateAttence(leave) && ShouldAddLeaveHour(leave, attence))
                            {
                                workHours += leaveHour;
                            }
                            
                            detail.Memo += GenerateLeaveMemo(leave, leaveHour);
                        }
                        // 判断是否有出差
                        if (hasBizTrip)
                        {
                            entity.Name = "出差";
                            entity.Type = CALENDAR_TYPE.BizTrp;
                            workHours = Normal_Work_Hours;
                            // 记录详情
                            var serialRecord = db.SerialRecord.Where(n => n.MessageID == bizTrip.MessageID).FirstOrDefault();
                            detail.Memo += string.Format("今日出差，出差编号:{0}", serialRecord == null ? string.Empty : serialRecord.Sn);
                        }
                        // 加入午休时间逻辑，迟到只比较到分
                        if (DateTime.Parse(attence.StartTime.Value.ToString("yyyy-MM-dd HH:mm:00")) > shouldStartTime)
                        {
                            if ((hasLeave || hasBizTrip) && workHours >= Normal_Work_Hours)
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
                            if (d == today || workHours >= Normal_Work_Hours)
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
            // 如果休息、节日、加班、工作时间或请假时间满足8小时，则视为正常
            if (entity.Type != CALENDAR_TYPE.Late && entity.Type != CALENDAR_TYPE.LeaveEarly && (entity.Type == CALENDAR_TYPE.Holiday || entity.Type == CALENDAR_TYPE.Rest || workHours >= 8 || leaveHour >= 8))
            {
                detail.isNormal = true;
            }
            detail.WorkHours = FillUpToHalfHour(Math.Round(workHours, 2));
            detail.Name = entity.Name;
            detail.Type = entity.Type;
        }
        #endregion

        #region 格式化出勤日志
        public static string GenerateLeaveMemo(XYD_Leave_Record leave, double leaveHour)
        {
            string memo;
            if (NeedUpdateAttence(leave))
            {
                memo = string.Format("今日{0} {1}", leave.Category, leave.StartDate.ToString("HH:mm:00"));
            }
            else
            {
                memo = string.Format("今日{0}{1}小时", leave.Category, leaveHour);
            }
            return memo;
        }
        #endregion

        #region 考勤是否应算到工作时间中
        public static bool ShouldAddLeaveHour(XYD_Leave_Record leave, XYD_Attence attence)
        {
            if (attence != null && attence.StartTime != null && attence.EndTime != null && leave.StartDate >= attence.StartTime.Value && leave.EndDate <= attence.EndTime.Value)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region 判断出勤是否该算入工作时间
        public static bool IsLeaveAsWork(XYD_Leave_Record leave)
        {
            string category = leave.Category;
            if (category == "加班" || category.Contains("补") || category == "外勤" || category.Contains("假"))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region 判断出勤是否需要更新考勤
        public static bool NeedUpdateAttence(XYD_Leave_Record leave)
        {
            string category = leave.Category;
            if (IsDayDate(leave.StartDate) && IsDayDate(leave.EndDate))
            {
                return false;
            }
            if (category.Contains("补") || category == "外勤")
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
            DateTime? StartTime = attence.StartTime;
            DateTime? EndTime = attence.EndTime;
            // 没有末次打卡
            if (EndTime == null)
            {
                if (d.Date == DateTime.Now.Date) // 如果是今天，则按照工作时间来计算
                {
                    EndTime = DateTime.Now;
                }
                else
                {
                    return workHours;
                }
            }
            // 计算工作时长
            var startTimeWithoutSec = DateTime.Parse(StartTime.Value.ToString("yyyy-MM-dd HH:mm:00"));
            var endTimeWithoutSec = DateTime.Parse(EndTime.Value.ToString("yyyy-MM-dd HH:mm:00"));
            workHours = CaculateTimeWithoutRest(startTimeWithoutSec, endTimeWithoutSec, restStartTime, restEndTime);
            return Math.Round(workHours, 2);
        }
        #endregion

        #region 计算排除掉午休的时间
        public static double CaculateTimeWithoutRest(DateTime startTime, DateTime endTime, DateTime restStartTime, DateTime restEndTime)
        {
            var workHours = 0.0d;
            // 计算工作时长
            if (endTime <= restStartTime || startTime >= restEndTime)
            {
                workHours = endTime.Subtract(startTime).TotalHours;
            }
            else if (startTime <= restStartTime && endTime >= restStartTime && endTime <= restEndTime)
            {
                workHours = restStartTime.Subtract(startTime).TotalHours;
            }
            else if (startTime >= restStartTime && startTime <= restEndTime && endTime >= restEndTime)
            {
                workHours = endTime.Subtract(restEndTime).TotalHours;
            }
            else if (startTime <= restStartTime && endTime >= restEndTime)
            {
                workHours = restStartTime.Subtract(startTime).TotalHours + endTime.Subtract(restEndTime).TotalHours;
            }
            else if (startTime >= restStartTime && endTime <= restEndTime)
            {
                return workHours;
            }
            return workHours;
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
        public static double GetRealLeaveHours(XYD_System_Config sysConfig, DateTime startTime, DateTime endTime)
        {
            var isDayDate = IsDayDate(startTime);
            var leaveHour = 0.0;

            if (isDayDate)
            {
                leaveHour = (endTime.Subtract(startTime).TotalDays + 1) * Normal_Work_Hours;
            }
            else
            {
                var startTimeWithoutSec = DateTime.Parse(startTime.ToString("yyyy-MM-dd HH:mm:00"));
                var endTimeWithoutSec = DateTime.Parse(endTime.ToString("yyyy-MM-dd HH:mm:00"));
                var restStartTime = DateTime.Parse(startTime.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.RestStartTime)));
                var restEndTime = DateTime.Parse(startTime.ToString(string.Format("yyyy-MM-dd {0}:00", sysConfig.RestEndTime)));
                leaveHour = CaculateTimeWithoutRest(startTimeWithoutSec, endTimeWithoutSec, restStartTime, restEndTime);
            }
            return leaveHour;
        }
        #endregion

        #region 判断时间是选到小时还是天
        public static bool IsDayDate(DateTime datetime)
        {
            if (datetime.Hour == 0 && datetime.Minute == 0 && datetime.Second == 0)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region 更新考勤记录，小时假才会进来
        public static void UpdateAttence(Employee employee, XYD_Leave_Record leave)
        {
            using (var db = new DefaultConnection())
            {
                var day = leave.StartDate.ToString("yyyy-MM-dd");
                var attence = db.Attence.Where(n => n.EmplNo == employee.EmplNO && n.Day == day).FirstOrDefault();
                if (attence == null)
                {
                    attence = new XYD_Attence();
                    attence.EmplNo = employee.EmplNO;
                    attence.EmplName = employee.EmplName;
                    if (UpdateOnlyHalfAttence(leave.Category))
                    {
                        if (leave.StartDate.Hour < 12)
                        {
                            attence.StartTime = leave.StartDate;
                        }
                        else
                        {
                            attence.EndTime = leave.EndDate;
                        }
                    }
                    else
                    {
                        attence.StartTime = leave.StartDate;
                        attence.EndTime = leave.EndDate;
                    }
                    attence.Day = leave.StartDate.ToString("yyyy-MM-dd");
                    attence.DeviceID = "新友达";
                    db.Attence.Add(attence);
                } else
                {
                    // 更新，最早考勤取最早，最晚考勤取最晚
                    // 如果是补打卡
                    if (attence.StartTime.Value > leave.StartDate)
                    {
                        if (attence.EndTime == null)
                        {
                            attence.EndTime = attence.StartTime;
                        }
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

        #region 获取用户所在地区系统配置
        public static XYD_System_Config GetSysConfigByUser(string EmplID)
        {
            using (var db = new DefaultConnection())
            {
                var workArea = OrgUtil.GetWorkArea(EmplID);
                var sysConfig = db.SystemConfig.Where(n => n.Area == workArea).FirstOrDefault();
                return sysConfig;
            }
        }
        #endregion

        #region 计算用户假期
        public static XYD_Vocation_Report CaculateVocation(string EmplID, DateTime startDate, DateTime endDate)
        {
            var db = new DefaultConnection();
            //var lastDayTime = CommonUtils.EndOfDay(endDate);
            // 年假
            var yearHour = GetHourByLeaveCategory(EmplID, "年假", startDate, endDate);
            // 加班
            var extraHour = GetHourByLeaveCategory(EmplID, "加班", startDate, endDate);
            // 事假
            var leaveHour = GetHourByLeaveCategory(EmplID, "事假", startDate, endDate);
            // 调休
            var changeHour = GetHourByLeaveCategory(EmplID, "调休", startDate, endDate);
            // 病假
            var sickHour = GetHourByLeaveCategory(EmplID, "病假", startDate, endDate);
            // 婚假
            var marryHour = GetHourByLeaveCategory(EmplID, "婚假", startDate, endDate);
            // 产假
            var birthHour = GetHourByLeaveCategory(EmplID, "产假", startDate, endDate);
            // 哺乳假
            var milkHour = GetHourByLeaveCategory(EmplID, "哺乳假", startDate, endDate);
            // 丧假
            var deadHour = GetHourByLeaveCategory(EmplID, "丧假", startDate, endDate);
            // 特殊调整
            var adjustHour = GetAdjustHourByUser(EmplID, startDate, endDate);
            return new XYD_Vocation_Report()
            {
                yearHour = FillUpToHalfHour(yearHour),
                extraHour = FillUpToHalfHour(extraHour),
                leaveHour = FillUpToHalfHour(leaveHour + changeHour),
                sickHour = FillUpToHalfHour(sickHour),
                marryHour = FillUpToHalfHour(marryHour),
                birthHour = FillUpToHalfHour(birthHour),
                milkHour = FillUpToHalfHour(milkHour),
                deadHour = FillUpToHalfHour(deadHour),
                adjustHour = FillUpToHalfHour(adjustHour)
            };
        }
        #endregion

        #region 根据类别获得出勤时间
        public static double GetHourByLeaveCategory(string EmplID, string category, DateTime startDate, DateTime endDate)
        {
            using(var db = new DefaultConnection())
            {
                //var lastDayTime = CommonUtils.EndOfDay(endDate);
                var records = db.LeaveRecord.Where(n => n.EmplID == EmplID
                                                    && n.Category == category
                                                    && n.Status == DEP_Constants.Leave_Status_YES
                                                    && ((n.StartDate <= startDate.Date && n.EndDate >= endDate.Date) || (n.StartDate >= startDate.Date && n.EndDate <= endDate))).ToList();
                return getTotalHours(records);
            }
        }
        #endregion

        #region 获取用户待调整时间
        public static double GetAdjustHourByUser(string EmplID, DateTime startDate, DateTime endDate)
        {
            using (var db = new DefaultConnection())
            {
                var sum = 0.0;
                var list = db.Adjust.Where(n => n.EmplID == EmplID && n.Date >= startDate && n.Date <= endDate).ToList();
                foreach(var item in list)
                {
                    sum += item.Hours;
                }
                return sum;
            }
        }
        #endregion

        #region 计算用户法定年假
        public static double GetUserYearHour(string EmplID)
        {
            using (var db = new DefaultConnection())
            {
                var yearHour = 0.0;
                var userCompanyInfo = db.UserCompanyInfo.Where(n => n.EmplID == EmplID).FirstOrDefault();
                if (userCompanyInfo != null)
                {
                    yearHour = CaculateYearRestDays(userCompanyInfo) * Normal_Work_Hours; // 小时制
                }
                return Math.Round(yearHour, 2);
            }  
        }
        #endregion

        #region 根据记录计算总时间
        public static double getTotalHours(List<XYD_Leave_Record> records)
        {
            var totalHours = 0d;
            foreach (var leave in records)
            {
                var sysConfig = GetSysConfigByUser(leave.EmplID);
                totalHours += GetRealLeaveHours(sysConfig, leave.StartDate, leave.EndDate);
            }
            return Math.Round(totalHours, 2);
        }
        #endregion

        #region 事假调休假打年假加班
        public static void CaculateLeftHour(double totalYearHour, double usedYearHour, double usedLeaveHour, double offTimeWork, double adjustHour, ref double leftYearHour, ref double leftLeaveHour, ref double leftOffTimeHour)
        {
            // 已剩年假
            var remainYearHour = totalYearHour - usedYearHour + adjustHour;
            // 计算剩余年假、剩余加班、剩余事假
            if (usedLeaveHour <= remainYearHour) // 先打年假
            {
                leftYearHour = remainYearHour - usedLeaveHour;
                leftOffTimeHour = offTimeWork;
                leftLeaveHour = 0.0d;
            }
            else
            {
                // 年假打空归零
                leftYearHour = 0.0d;
                leftOffTimeHour = offTimeWork;
                // 计算剩余需要打的时间:请假时间-剩余年假
                var remainLeaveHour = usedLeaveHour - remainYearHour;

                if (remainLeaveHour <= leftOffTimeHour) // 再打加班
                {
                    leftOffTimeHour -= remainLeaveHour;
                    leftLeaveHour = 0.0d;
                }
                else // 扣完年假、加班，变成负数
                {
                    leftLeaveHour = leftOffTimeHour - remainLeaveHour;
                    leftOffTimeHour = 0.0d;
                }
            }
        }
        #endregion

        #region 判断是否只更新上下午打卡
        public static bool UpdateOnlyHalfAttence(string Category)
        {
            if (Category == "补打卡")
            {
                return true;
            }
            return false;
        }
        #endregion

        #region 补满时间到半小时
        public static double FillUpToHalfHour(double hour)
        {
            return Math.Ceiling(hour * 2) / 2;
        }
        #endregion
    }
}