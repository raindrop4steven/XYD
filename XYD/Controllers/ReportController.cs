using Appkiz.Library.Security;
using Appkiz.Library.Security.Authentication;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Entity;
using XYD.Models;

namespace XYD.Controllers
{
    public class ReportController : Controller
    {

        OrgMgr orgMgr = new OrgMgr();

        #region 考勤统计
        [Authorize]
        public ActionResult Calendar(DateTime BeginDate, DateTime EndDate)
        {
            try
            {
                // 检查用户是否具有领导权限
                var employee = (User.Identity as AppkizIdentity).Employee;
                var isLeader = PermUtil.CheckPermission(employee.EmplID, DEP_Constants.Module_Information_Code, DEP_Constants.Perm_Info_Leader);
                if (!isLeader)
                {
                    return ResponseUtil.Error("您没有权限查看数据");
                }
                if (OrgUtil.CheckRole(employee.EmplID, "查看所有机构报表"))
                {
                    var CEOEmplID = ConfigurationManager.AppSettings["CEOEmplID"];
                    employee = orgMgr.GetEmployee(CEOEmplID);
                }
                List<Employee> employees = OrgUtil.GetChildrenDeptRecursive(employee.DeptID);
                var results = new List<XYD_Calendar_Report>();
                // 计算应上班天数
                var shouldWorkDays = CalendarUtil.CaculateShouldWorkDays(BeginDate, EndDate);
                foreach(var user in employees)
                {
                    var calendarResult = CalendarUtil.CaculateUserCalendar(user, BeginDate, EndDate);
                    results.Add(new XYD_Calendar_Report() {
                        EmplID = user.EmplID,
                        EmplName = user.EmplName,
                        DeptName = user.DeptName,
                        EmplNo = user.EmplNO,
                        Position = user.DeptAndPosStr,
                        ShouldWorkDays = shouldWorkDays,
                        summary = calendarResult.summary
                    });
                }
                return ResponseUtil.OK(results);
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 考勤详情接口
        [Authorize]
        public ActionResult CalendarDetail(string EmplID, DateTime BeginDate, DateTime EndDate)
        {
            // 检查用户是否具有领导权限
            var employee = (User.Identity as AppkizIdentity).Employee;
            var isLeader = PermUtil.CheckPermission(employee.EmplID, DEP_Constants.Module_Information_Code, DEP_Constants.Perm_Info_Leader);
            if (!isLeader)
            {
                return ResponseUtil.Error("您没有权限查看数据");
            }
            var calendarResult = CalendarUtil.CaculateUserCalendarDetail(orgMgr.GetEmployee(EmplID), BeginDate, EndDate);
            return ResponseUtil.OK(calendarResult);
        }
        #endregion

        #region 每日考勤查询
        [Authorize]
        public ActionResult DailyCalendar(DateTime BeginDate, string Area)
        {
            try
            {
                var AreaName = string.Empty;
                var EndDate = BeginDate;
                // 检查用户是否具有领导权限
                var employee = (User.Identity as AppkizIdentity).Employee;
                var isLeader = PermUtil.CheckPermission(employee.EmplID, DEP_Constants.Module_Information_Code, DEP_Constants.Perm_Info_Leader);
                if (!isLeader)
                {
                    return ResponseUtil.Error("您没有权限查看数据");
                }
                if (OrgUtil.CheckRole(employee.EmplID, "查看所有机构报表"))
                {
                    var CEOEmplID = ConfigurationManager.AppSettings["CEOEmplID"];
                    employee = orgMgr.GetEmployee(CEOEmplID);
                }
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
                List<Employee> employees = OrgUtil.GetChildrenDeptRecursive(employee.DeptID);
                var results = new List<object>();
                // 计算应上班天数
                var shouldWorkDays = CalendarUtil.CaculateShouldWorkDays(BeginDate, EndDate);
                foreach (var user in employees)
                {
                    if (!string.IsNullOrEmpty(AreaName) && !OrgUtil.CheckRole(user.EmplID, AreaName))
                    {
                        continue;
                    }
                    var calendarResult = CalendarUtil.CaculateUserCalendarDetail(user, BeginDate, EndDate);
                    results.Add(new {
                        EmplID = user.EmplID,
                        EmplName = user.EmplName,
                        DeptName = user.DeptName,
                        EmplNo = user.EmplNO,
                        Position = user.DeptAndPosStr,
                        calendarResult = calendarResult
                    });
                }
                return ResponseUtil.OK(results);
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 工资统计
        [HttpPost]
        public ActionResult Salary(DateTime BeginDate, DateTime EndDate)
        {
            try
            {
                // 检查用户是否具有领导权限
                var employee = (User.Identity as AppkizIdentity).Employee;
                var isLeader = PermUtil.CheckPermission(employee.EmplID, DEP_Constants.Module_Information_Code, DEP_Constants.Perm_Info_Leader);
                string sql = string.Empty;
                string orderBy = string.Empty;
                var selectClause = @" cPsn_Num, cPsn_Name, b.cDepName, F_9, F_10, F_12, F_11, F_26, F_1, F_13, F_14, F_15, F_16, F_17,
	                                        F_18,
	                                        F_19,
	                                        F_20,
	                                        F_21,
	                                        F_22,
	                                        F_23,
	                                        F_1102,
	                                        F_1103,
	                                        F_1104,
	                                        F_1105,
	                                        F_1106,
	                                        F_1108,
	                                        F_1112,
	                                        F_1116,
	                                        F_1115,
	                                        F_1114,
	                                        F_1113,
	                                        F_1001,
	                                        F_1002,
	                                        F_6,
	                                        F_1003,
	                                        F_25,
	                                        F_24,
	                                        F_1004,
	                                        F_2,
	                                        F_3,
                                            b.cDepCode,
                                            iYear,
                                            iMonth
                                        FROM
	                                        WA_GZData a
	                                        LEFT JOIN Department b ON b.cDepCode = a.cDept_Num ";
                if (isLeader)
                {
                    // 无锡sql
                    var wxSql = GetLeaderQuerySql(selectClause, BeginDate, EndDate, "001", string.Empty);
                    // 上海sql
                    var shSql = GetLeaderQuerySql(selectClause, BeginDate, EndDate, "002", string.Empty);
                    sql = string.Format(@"select 
                                            DISTINCT
	                                            a.cPsn_Num,
	                                            a.cPsn_Name,
	                                            a.cDepName,
	                                            SUM ( a.F_9 ) AS F_9,
	                                            SUM ( a.F_10 ) AS F_10,
	                                            SUM ( a.F_12 ) AS F_12,
	                                            SUM ( a.F_11 ) AS F_11,
	                                            SUM ( a.F_26 ) AS F_26,
	                                            SUM ( a.F_1 ) AS F_1,
	                                            SUM ( a.F_13 ) AS F_13,
	                                            SUM ( a.F_14 ) AS F_14,
	                                            SUM ( a.F_15 ) AS F_15,
	                                            SUM ( a.F_16 ) AS F_16,
	                                            SUM ( a.F_17 ) AS F_17,
	                                            SUM ( a.F_18 ) AS F_18,
	                                            SUM ( a.F_19 ) AS F_19,
	                                            SUM ( a.F_20 ) AS F_20,
	                                            SUM ( a.F_21 ) AS F_21,
	                                            SUM ( a.F_22 ) AS F_22,
	                                            SUM ( a.F_23 ) AS F_23,
	                                            SUM ( a.F_1102 ) AS F_1102,
	                                            SUM ( a.F_1103 ) AS F_1103,
	                                            SUM ( a.F_1104 ) AS F_1104,
	                                            SUM ( a.F_1105 ) AS F_1105,
	                                            SUM ( a.F_1106 ) AS F_1106,
	                                            SUM ( a.F_1108 ) AS F_1108,
	                                            SUM ( a.F_1112 ) AS F_1112,
	                                            SUM ( a.F_1116 ) AS F_1116,
	                                            SUM ( a.F_1115 ) AS F_1115,
	                                            SUM ( a.F_1114 ) AS F_1114,
	                                            SUM ( a.F_1113 ) AS F_1113,
	                                            SUM ( a.F_1001 ) AS F_1001,
	                                            SUM ( a.F_1002 ) AS F_1002,
	                                            SUM ( a.F_6 ) AS F_6,
	                                            SUM ( a.F_1003 ) AS F_1003,
	                                            SUM ( a.F_25 ) AS F_25,
	                                            SUM ( a.F_24 ) AS F_24,
	                                            SUM ( a.F_1004 ) AS F_1004,
	                                            SUM ( a.F_2 ) AS F_2,
	                                            SUM ( a.F_3 ) AS F_3,
	                                            a.cDepCode 
                                          from ({0} union {1}) a GROUP BY
	                                        a.cPsn_Num,
	                                        a.cPsn_Name,
	                                        a.cDepName,
	                                        a.cDepCode 
                                        ORDER BY
	                                        a.cDepCode", wxSql, shSql);
                }
                else
                {
                    return ResponseUtil.Error("您没有权限查看数据");
                }
                // SQL 连接字符串
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["YongYouConnection"].ConnectionString;
                // 记录总页数
                var list = DbUtil.ExecuteSqlCommand(connectionString, sql, DbUtil.GetReportSalary);

                return ResponseUtil.OK(new
                {
                    results = list
                });
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }

        public string GetLeaderQuerySql(string selectClause, DateTime BeginDate, DateTime EndDate, string Area, string UserName)
        {
            var AreaName = string.Empty;
            if (Area == "001")
            {
                AreaName = "无锡";
            }
            else
            {
                AreaName = "上海";
            }
            var areaCondition = string.Format(@" INNER JOIN ORG_EmplRole b on a.EmplID = b.EmplID INNER JOIN ORG_Role c on b.RoleID = c.RoleID and c.RoleName = '{0}'", AreaName);
            List<Employee> employees = orgMgr.FindEmployeeBySQL(string.Format(@"SELECT
                                                                                            *
                                                                                        FROM
                                                                                            ORG_Employee a
                                                                                            {0}
                                                                                        WHERE
                                                                                            a.EmplName LIKE '%{1}%'
                                                                                            AND a.EmplNO != ''", areaCondition, UserName));
            List<string> idList = employees.Select(n => "'" + n.EmplNO + "'").ToList();
            string inClause = string.Join(",", idList);
            // 构造查询 
            var sql = string.Format(@"SELECT
                                            {0}
                                            WHERE
	                                            cGZGradeNum LIKE '{6}%' 
	                                            AND cPsn_Num in ({1})
	                                            AND iYear >= {2}
                                                AND iYear <= {3}
	                                            AND iMonth >= {4}
	                                            AND iMonth <= {5} ", selectClause, inClause, BeginDate.Year, EndDate.Year, BeginDate.Month, EndDate.Month, Area);
            return sql;
        }
        #endregion

        #region 备用金统计
        public ActionResult BackupMoney(DateTime BeginDate, DateTime EndDate)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                var isLeader = PermUtil.CheckPermission(employee.EmplID, DEP_Constants.Module_Information_Code, DEP_Constants.Perm_Info_Leader);
                if (!isLeader)
                {
                    return ResponseUtil.Error("您没有权限查看数据");
                }
                var sql = string.Format(@"SELECT DISTINCT
	                            EmplName,
	                            DeptName,
	                            SUM ( Amount ) AS Amount 
                            FROM
	                            XYD_BackupMoney 
                            WHERE
                                CreateTime >= '{0}'
                            AND
                                CreateTime < '{1}'
                            GROUP BY
	                            EmplName,
	                            DeptName 
                            ORDER BY
	                            Amount DESC", BeginDate.Date, EndDate.AddMonths(1));
                var list = DbUtil.ExecuteSqlCommand(sql, DbUtil.BackupMoneyReport);
                decimal total = 0;
                foreach(XYD_BackupMoneyReport item in list)
                {
                    total += item.Amount;
                }
                return ResponseUtil.OK(new
                {
                    results = list,
                    total = total
                });
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 假期统计
        [Authorize]
        public ActionResult Leave(string Year, string Area)
        {
            try
            {
                // 检查用户是否具有领导权限
                var employee = (User.Identity as AppkizIdentity).Employee;
                var isLeader = PermUtil.CheckPermission(employee.EmplID, DEP_Constants.Module_Information_Code, DEP_Constants.Perm_Info_Leader);
                if (!isLeader)
                {
                    return ResponseUtil.Error("您没有权限查看数据");
                }
                var db = new DefaultConnection();
                // 选择地区非空，则进行地区人员筛选
                var AreaName = string.Empty;
                var areaCondition = string.Empty;
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
                    areaCondition = string.Format(@" INNER JOIN ORG_EmplRole b on a.EmplID = b.EmplID INNER JOIN ORG_Role c on b.RoleID = c.RoleID and c.RoleName = '{0}'", AreaName);
                }
                List<string> employeesWithoutDept = orgMgr.FindEmployeesWithoutDept().Select(n => n.EmplID).ToList();
                List<string> excludeReportUsers = orgMgr.FindEmployeeBySQL(@"SELECT
	                                                                            * 
                                                                            FROM
	                                                                            ORG_Employee a
	                                                                            INNER JOIN ORG_EmplRole b ON a.EmplID = b.EmplID
	                                                                            INNER JOIN ORG_Role c ON b.RoleID = c.RoleID 
	                                                                            AND c.RoleName = '非统计用户'").Select(n => n.EmplID).ToList();
                List<Employee> employees = orgMgr.FindEmployeeBySQL(string.Format(@"SELECT
                                                                                            *
                                                                                        FROM
                                                                                            ORG_Employee a
                                                                                            {0}
                                                                                        WHERE
                                                                                            a.EmplNO != ''
                                                                                        AND
                                                                                            a.EmplID != '100001'
                                                                                        AND a.EmplEnabled = 1", areaCondition));
                var results = new List<object>();
                var startYearDate = DateTime.Parse(string.Format("{0}-01-01", Year));
                var endYearDate = DateTime.Parse(string.Format("{0}-12-31 23:59:59", Year));
                // 根据每个用户，计算剩余年假、剩余加班、剩余事假
                foreach(var user in employees)
                {
                    if (employeesWithoutDept.Contains(user.EmplID) || excludeReportUsers.Contains(user.EmplID))
                    {
                        continue;
                    }
                    // 计算总年假
                    var userCompanyInfo = db.UserCompanyInfo.Where(n => n.EmplID == user.EmplID).FirstOrDefault();
                    if (userCompanyInfo == null)
                    {
                        return ResponseUtil.Error(string.Format("请补全{0}信息", user.EmplName));
                    }
                    var restYear = CalendarUtil.CaculateYearRestDays(userCompanyInfo) * 8; // 小时制
                    var leaveAndAdjust = new List<string>() { "事假", "调休" };
                    // 计算已使用年假
                    var usedRestYear = db.LeaveRecord.Where(n => n.EmplID == user.EmplID && n.Category == "年假" && n.StartDate >= startYearDate && n.StartDate <= endYearDate).ToList().Select(n => n.EndDate.Subtract(n.StartDate).TotalHours).Sum();
                    // 计算已加班
                    var offTimeWork = db.LeaveRecord.Where(n => n.EmplID == user.EmplID && n.Category == "加班" && n.StartDate >= startYearDate && n.StartDate <= endYearDate).ToList().Select(n => n.EndDate.Subtract(n.StartDate).TotalHours).Sum();
                    // 已申请事假
                    var leaveRecords = db.LeaveRecord.Where(n => n.EmplID == user.EmplID && leaveAndAdjust.Contains(n.Category) && n.StartDate >= startYearDate && n.StartDate <= endYearDate).ToList();
                    var totalLeaveHours = 0d;
                    foreach(var leave in leaveRecords)
                    {
                        totalLeaveHours += CalendarUtil.GetRealLeaveHours(user.EmplID, leave.StartDate, leave.EndDate);
                    }
                    // 计算结果
                    var leftYearHour = 0.0d;
                    var leftOffWorkHour = 0.0d;
                    var leftLeaveHour = 0.0d;
                    // 计算剩余年假、剩余加班、剩余事假
                    if (totalLeaveHours <= (restYear - usedRestYear)) // 先打年假
                    {
                        leftYearHour = restYear - usedRestYear - totalLeaveHours;
                        leftOffWorkHour = offTimeWork;
                        leftLeaveHour = 0.0d;
                    }
                    else
                    {
                        leftYearHour = 0.0d;
                        if (totalLeaveHours-(restYear-usedRestYear) <= leftOffWorkHour) // 再打加班
                        {
                            leftOffWorkHour -= totalLeaveHours - (restYear - usedRestYear);
                            leftLeaveHour = 0.0d;
                        }
                        else
                        {
                            leftLeaveHour = leftOffWorkHour - (totalLeaveHours - (restYear - usedRestYear));
                            leftOffWorkHour = 0.0d;
                        }
                    }
                    results.Add(new
                    {
                        EmplID = user.EmplID,
                        EmplName = user.EmplName,
                        DeptName = user.DeptName,
                        EmplNo = user.EmplNO,
                        Position = user.DeptAndPosStr,
                        LeftYearHour = Math.Round(leftYearHour, 2),
                        LeftWorfHour = Math.Round(leftOffWorkHour, 2),
                        LeftLeaveHour = Math.Round(leftLeaveHour, 2),
                        RestYearHour = restYear
                    });
                }
                return ResponseUtil.OK(results);
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 假期统计明细
        [Authorize]
        public ActionResult LeaveDetail(string EmplID, string Year)
        {
            try
            {
                var employee = orgMgr.GetEmployee(EmplID);
                var db = new DefaultConnection();
                var yearStartDate = DateTime.Parse(string.Format("{0}-01-01", Year));
                var now = DateTime.Now;
                var results = new List<object>();
                // 累计使用的年假、加班
                var sumYearHour = 0.0d;
                var sumOffWorkHour = 0.0d;
                var sumDeltaHour = 0.0d;
                for (int i = 0; i < now.Month; i++)
                {
                    var startMonthDate = yearStartDate.AddMonths(i);
                    var endMonthDate = startMonthDate.AddMonths(1);
                    // 计算总年假
                    var userCompanyInfo = db.UserCompanyInfo.Where(n => n.EmplID == EmplID).FirstOrDefault();
                    var restYear = CalendarUtil.CaculateYearRestDays(userCompanyInfo) * 8; // 小时制
                    // 年假
                    var usedRestYear = db.LeaveRecord.Where(n => n.EmplID == EmplID && n.Category == "年假" && n.StartDate >= startMonthDate && n.StartDate <= endMonthDate).ToList().Select(n => n.EndDate.Subtract(n.StartDate).TotalDays*8).Sum();
                    // 加班
                    var offTimeWork = db.LeaveRecord.Where(n => n.EmplID == EmplID && n.Category == "加班" && n.StartDate >= startMonthDate && n.StartDate <= endMonthDate).ToList().Select(n => n.EndDate.Subtract(n.StartDate).TotalHours).Sum();
                    // 事假
                    var leaveRecords = db.LeaveRecord.Where(n => n.EmplID == EmplID && n.Category == "事假" && n.StartDate >= startMonthDate && n.StartDate <= endMonthDate).ToList();
                    var totalLeaveHours = getTotalHours(leaveRecords);
                    // 病假
                    var sickRecords = db.LeaveRecord.Where(n => n.EmplID == EmplID && n.Category == "病假" && n.StartDate >= startMonthDate && n.StartDate <= endMonthDate).ToList();
                    var totalSickHours = getTotalHours(sickRecords);
                    // 婚假
                    var marryRecords = db.LeaveRecord.Where(n => n.EmplID == EmplID && n.Category == "婚假" && n.StartDate >= startMonthDate && n.StartDate <= endMonthDate).ToList();
                    var totalMarryHours = getTotalHours(marryRecords);
                    // 产假
                    var birthRecords = db.LeaveRecord.Where(n => n.EmplID == EmplID && n.Category == "产假" && n.StartDate >= startMonthDate && n.StartDate <= endMonthDate).ToList();
                    var totalBirthHours = getTotalHours(birthRecords);
                    // 哺乳假
                    var milkRecords = db.LeaveRecord.Where(n => n.EmplID == EmplID && n.Category == "哺乳假" && n.StartDate >= startMonthDate && n.StartDate <= endMonthDate).ToList();
                    var totalMilkHours = getTotalHours(milkRecords);
                    // 丧假
                    var sadRecords = db.LeaveRecord.Where(n => n.EmplID == EmplID && n.Category == "丧假" && n.StartDate >= startMonthDate && n.StartDate <= endMonthDate).ToList();
                    // TODO: 该月差额小时
                    var deltaHour = 0.0d;
                    var totalSadHours = getTotalHours(sadRecords);
                    sumYearHour += usedRestYear;
                    sumOffWorkHour += offTimeWork;
                    sumDeltaHour += deltaHour;
                    // 计算结果
                    var leftYearHour = 0.0d;
                    var leftOffWorkHour = 0.0d;
                    var leftLeaveHour = 0.0d;
                    // 计算剩余年假、剩余加班、剩余事假
                    if (totalLeaveHours <= (restYear - sumYearHour - sumDeltaHour)) // 先打年假
                    {
                        leftYearHour = restYear - sumYearHour - totalLeaveHours - sumDeltaHour;
                        leftOffWorkHour = offTimeWork;
                        leftLeaveHour = 0.0d;
                    }
                    else
                    {
                        leftYearHour = 0.0d;
                        if (totalLeaveHours - (restYear - sumYearHour - sumDeltaHour) <= leftOffWorkHour) // 再打加班
                        {
                            leftOffWorkHour -= totalLeaveHours - (restYear - sumYearHour - sumDeltaHour);
                            leftLeaveHour = 0.0d;
                        }
                        else
                        {
                            leftLeaveHour = leftOffWorkHour - (totalLeaveHours - (restYear - sumYearHour - sumDeltaHour));
                            leftOffWorkHour = 0.0d;
                        }
                    }
                    results.Add(new
                    {
                        Date = startMonthDate.ToString("yyyyMM"),
                        EmplNo = employee.EmplNO,
                        EmplName = employee.EmplName,
                        OffTimeWork = Math.Round(offTimeWork),
                        LeaveHours = Math.Round(totalLeaveHours),
                        SickHours = Math.Round(totalSickHours),
                        YearHours = Math.Round(usedRestYear),
                        MarryHours = Math.Round(totalMarryHours),
                        BirthHours = Math.Round(totalBirthHours),
                        MilkHours = Math.Round(totalMilkHours),
                        SadHours = Math.Round(totalSadHours),
                        LeftYearHour = Math.Round(leftYearHour),
                        LeftLeaveHour = Math.Round(leftLeaveHour),
                        DeltaHour = Math.Round(deltaHour)
                    });
                }
                return ResponseUtil.OK(results);
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 根据记录计算总时间
        private double getTotalHours(List<XYD_Leave_Record> records)
        {
            var totalHours = 0d;
            foreach (var leave in records)
            {
                totalHours += CalendarUtil.GetRealLeaveHours(leave.EmplID, leave.StartDate, leave.EndDate);
            }
            return totalHours;
        }
        #endregion
    }
}