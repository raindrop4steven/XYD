using Appkiz.Library.Security;
using Appkiz.Library.Security.Authentication;
using DocumentFormat.OpenXml.Office.CustomUI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
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
        public ActionResult Calendar(DateTime BeginDate, DateTime EndDate, string Area)
        {
            try
            {
                // 检查用户是否具有领导权限
                var AreaName = string.Empty;
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
                var results = new List<XYD_Calendar_Report>();

                List<string> excludeReportUsers = OrgUtil.GetUsersByRole("非考勤统计用户").Select(n => n.EmplID).ToList();
                // 计算应上班天数
                var shouldWorkDays = CalendarUtil.CaculateShouldWorkDays(BeginDate, EndDate);
                foreach(var user in employees)
                {
                    if (excludeReportUsers.Contains(user.EmplID))
                    {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(AreaName) && !OrgUtil.CheckRole(user.EmplID, AreaName))
                    {
                        continue;
                    }
                    var calendarResult = CalendarUtil.CaculateUserCalendarDetail(user, BeginDate, EndDate);
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
            if (string.IsNullOrEmpty(EmplID))
            {
                EmplID = employee.EmplID;
            }
            else
            {
                var isLeader = PermUtil.CheckPermission(employee.EmplID, DEP_Constants.Module_Information_Code, DEP_Constants.Perm_Info_Leader);
                if (!isLeader)
                {
                    return ResponseUtil.Error("您没有权限查看数据");
                }
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
                List<string> excludeReportUsers = OrgUtil.GetUsersByRole("非考勤统计用户").Select(n => n.EmplID).ToList();
                // 计算应上班天数
                var shouldWorkDays = CalendarUtil.CaculateShouldWorkDays(BeginDate, EndDate);
                foreach (var user in employees)
                {
                    if (excludeReportUsers.Contains(user.EmplID))
                    {
                        continue;
                    }
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
                throw e;
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
                    var specialSql = GetSpecialQuerySql(selectClause, BeginDate, EndDate, string.Empty, string.Empty);

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
                                          from ({0} union {1} union {2}) a GROUP BY
	                                        a.cPsn_Num,
	                                        a.cPsn_Name,
	                                        a.cDepName,
	                                        a.cDepCode 
                                        ORDER BY
	                                        a.cDepCode", wxSql, shSql, specialSql);
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
            //List<string> idList = employees.Select(n => "'" + n.EmplNO + "'").ToList();
            List<string> excludeIds = OrgUtil.GetUsersByRole("部门与工资不同人员").Select(n => n.EmplID).ToList();
            List<string> idList = employees.Where(n => !excludeIds.Contains(n.EmplID)).Select(n => "'" + n.EmplNO + "'").ToList();
            string inClause = "''";
            if (idList.Count > 0)
            {
                inClause = string.Join(",", idList);
            }
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

        public string GetSpecialQuerySql(string selectClause, DateTime BeginDate, DateTime EndDate, string Area, string UserName)
        {
            // 部门反转
            if (Area == "001")
            {
                Area = "002";
            }
            else
            {
                Area = "001";
            }
            List<string> idList = OrgUtil.GetUsersByRole("部门与工资不同人员").Where(n => n.EmplName.Contains(UserName)).Select(n => "'" + n.EmplNO + "'").ToList();
            string inClause = "''";
            if (idList.Count > 0)
            {
                inClause = string.Join(",", idList);
            }
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
                                CreateTime <= '{1}'
                            GROUP BY
	                            EmplName,
	                            DeptName 
                            ORDER BY
	                            Amount DESC", BeginDate.Date, CommonUtils.EndOfDay(EndDate));
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
                List<string> excludeReportUsers = OrgUtil.GetUsersByRole("非统计用户").Select(n => n.EmplID).ToList(); 
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
                    // 剩余时间纬度
                    var leftYearHour = 0.0d;
                    var leftOffTimeHour = 0.0d;
                    var leftLeaveHour = 0.0d;

                    // 计算总年假
                    var userCompanyInfo = db.UserCompanyInfo.Where(n => n.EmplID == user.EmplID).FirstOrDefault();
                    var totalYearHour = CalendarUtil.GetUserYearHour(user.EmplID);
                    var vocationReport = CalendarUtil.CaculateVocation(user.EmplID, startYearDate, endYearDate);

                    // 已使用年假
                    var usedYearHour = vocationReport.yearHour;
                    // 已使用事假、调休
                    var usedLeaveHour = vocationReport.leaveHour;
                    // 加班时间
                    var offTimeWork = vocationReport.extraHour;
                    // 特殊调整
                    var adjustHour = vocationReport.adjustHour;
                    // 开始计算剩余
                    CalendarUtil.CaculateLeftHour(totalYearHour, usedYearHour, usedLeaveHour, offTimeWork, adjustHour, ref leftYearHour, ref leftLeaveHour, ref leftOffTimeHour);

                    results.Add(new
                    {
                        EmplID = user.EmplID,
                        EmplName = user.EmplName,
                        DeptName = user.DeptName,
                        EmplNo = user.EmplNO,
                        Position = user.DeptAndPosStr,
                        LeftYearHour = Math.Round(leftYearHour, 2),
                        LeftWorfHour = Math.Round(leftOffTimeHour, 2),
                        LeftLeaveHour = Math.Round(leftLeaveHour, 2),
                        AdjustHour = adjustHour,
                        RestYearHour = Math.Round(totalYearHour, 2)
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
                var sumUsedYearHour = 0.0d;
                var sumOffTimeWork = 0.0d;
                var sumAdjustHour = 0.0d;
                var sumUsedLeaveHour = 0.0d;
                for (int i = 0; i < now.Month; i++)
                {
                    var startMonthDate = yearStartDate.AddMonths(i);
                    var endMonthDate = startMonthDate.AddMonths(1).AddTicks(-1);

                    // 剩余时间纬度
                    var leftYearHour = 0.0d;
                    var leftOffTimeHour = 0.0d;
                    var leftLeaveHour = 0.0d;

                    // 计算总年假
                    var userCompanyInfo = db.UserCompanyInfo.Where(n => n.EmplID == EmplID).FirstOrDefault();
                    var totalYearHour = CalendarUtil.GetUserYearHour(EmplID);
                    var vocationReport = CalendarUtil.CaculateVocation(EmplID, startMonthDate, endMonthDate);

                    // 已使用年假
                    var usedYearHour = vocationReport.yearHour;
                    // 已使用事假、调休
                    var usedLeaveHour = vocationReport.leaveHour;
                    // 加班时间
                    var offTimeWork = vocationReport.extraHour;
                    // 特殊调整
                    var adjustHour = vocationReport.adjustHour;

                    sumUsedYearHour += usedYearHour;
                    sumOffTimeWork += offTimeWork;
                    sumUsedLeaveHour += usedLeaveHour;
                    sumAdjustHour += adjustHour;
                    // 开始计算剩余
                    CalendarUtil.CaculateLeftHour(totalYearHour, sumUsedYearHour, sumUsedLeaveHour, sumOffTimeWork, sumAdjustHour,
                        ref leftYearHour, ref leftLeaveHour, ref leftOffTimeHour);

                    results.Add(new
                    {
                        Date = startMonthDate.ToString("yyyyMM"),
                        EmplNo = employee.EmplNO,
                        EmplName = employee.EmplName,
                        OffTimeWork = offTimeWork,
                        LeaveHours = Math.Round(vocationReport.leaveHour, 2),
                        SickHours = Math.Round(vocationReport.sickHour, 2),
                        YearHours = Math.Round(vocationReport.yearHour, 2),
                        MarryHours = Math.Round(vocationReport.marryHour, 2),
                        BirthHours = Math.Round(vocationReport.birthHour, 2),
                        MilkHours = Math.Round(vocationReport.milkHour, 2),
                        SadHours = Math.Round(vocationReport.deadHour, 2),
                        LeftYearHour = Math.Round(leftYearHour, 2),
                        LeftOffTimeHour = Math.Round(leftOffTimeHour, 2),
                        DeltaHour = Math.Round(adjustHour, 2)
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

        #region 员工信息统计
        [Authorize]
        public ActionResult Employee(string Area, string Name)
        {
            try
            {
                var AreaName = string.Empty;
                var db = new DefaultConnection();
                string[] educationOrder = { "博士", "硕士", "本科", "专科", "高中" };
                var conditionSql = string.Empty;
                if (!string.IsNullOrEmpty(Name))
                {
                    conditionSql = string.Format(@" and a.EmplName like '%{0}%'", Name);
                }

                var sql = string.Format(@"SELECT
	                            ISNULL( a.EmplID, '' ) AS EmplID,
	                            ISNULL( a.EmplName, '' ) AS EmplName,
	                            a.EmplSex AS EmplSex,
	                            ISNULL( a.EmplNO, '' ) AS EmplNO,
	                            ISNULL( b.DeptName, '' ) AS DeptName,
	                            ISNULL( d.PositionName, '' ) AS PositionName,
	                            a.EmplBirth,
	                            f.CredNo,
	                            e.BankNo,
	                            e.EmployeeDate,
	                            e.FormalDate,
	                            e.ContractDate,
	                            e.SocialInsuranceTotalMonth,
	                            f.Residence,
	                            f.CurrentAddress 
                            FROM
	                            ORG_Employee a
	                            LEFT JOIN ORG_Department b ON a.DeptID = b.DeptID
	                            INNER JOIN ORG_EmplDept c ON c.EmplID = a.EmplID
	                            INNER JOIN ORG_Position d ON d.PositionID = c.PosID
	                            LEFT JOIN XYD_UserCompanyInfo e ON a.EmplID = e.EmplID
	                            LEFT JOIN XYD_UserInfo f ON a.EmplID = f.EmplID 
                            WHERE
	                            a.EmplEnabled = 1
                                {0}
                            ORDER BY
	                            GlobalSortNo DESC", conditionSql);
                
                var results = DbUtil.ExecuteSqlCommand(sql, DbUtil.GetUserReport);
                var filterResults = new List<XYD_UserReport>();
                List<string> excludeReportUsers = OrgUtil.GetUsersByRole("非员工统计用户").Select(n => n.EmplID).ToList();
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
                foreach (XYD_UserReport item in results)
                {
                    if (excludeReportUsers.Contains(item.EmplID))
                    {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(AreaName) && !OrgUtil.CheckRole(item.EmplID, AreaName))
                    {
                        continue;
                    }
                    // 获取联系方式
                    var education = db.Education.Where(n => n.EmplID == item.EmplID).ToList().OrderBy(n => Array.IndexOf(educationOrder, n.Level)).FirstOrDefault();
                    if (education != null)
                    {
                        item.Education = education.Level;
                    }
                    // 获取联系方式
                    var mobileContact = orgMgr.FindEmployeeContactInfo("EmplID=@EmplID and ContactInfoTypeID=@ContactInfoTypeID", new System.Collections.Hashtable()
                          {
                            {
                              "@EmplID",
                              item.EmplID
                            },
                            {
                                "@ContactInfoTypeID",
                                "mobile"
                            }
                          }, string.Empty, 0, 0).FirstOrDefault();
                    if (mobileContact != null)
                    {
                        item.Mobile = GetContactValue(mobileContact.ContactInfoValue);
                    }
                    var emailContact = orgMgr.FindEmployeeContactInfo("EmplID=@EmplID and ContactInfoTypeID=@ContactInfoTypeID", new System.Collections.Hashtable()
                          {
                            {
                              "@EmplID",
                              item.EmplID
                            },
                            {
                                "@ContactInfoTypeID",
                                "email"
                            }
                          }, string.Empty, 0, 0).FirstOrDefault();
                    if (emailContact != null)
                    {
                        item.Email = GetContactValue(emailContact.ContactInfoValue);
                    }
                    // 紧急联系人
                    var emergency = db.Contact.Where(n => n.EmplID == item.EmplID).FirstOrDefault();
                    if (emergency != null)
                    {
                        item.EmergencyContact = emergency.Name;
                        item.EmergencyMobile = emergency.Contact;
                    }
                    filterResults.Add(item);
                }
                return ResponseUtil.OK(filterResults);
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }

        private string GetContactValue(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlElement root = doc.DocumentElement;
            return root.FirstChild.FirstChild.Attributes[1].Value;
        }
        #endregion

        #region 出勤记录统计
        [Authorize]
        public ActionResult SearchLeave(string Year, string Area, string Name, string Category,  int Page, int Size)
        {
            try
            {
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
                // 年份
                var startYearDate = string.Format("{0}-01-01", Year);
                var endYearDate =string.Format("{0}-12-31 23:59:59", Year);
                var whereCondition = string.Format("and a.StartDate >= '{0}' and a.EndDate <= '{1}' and d.EmplName like '%{2}%' and a.Category like '{3}%'", startYearDate, endYearDate, Name, Category);
                
                var sql = string.Format(@"SELECT
                                d.EmplID,
	                            d.EmplName,
	                            d.EmplNO,
	                            a.Category,
	                            a.StartDate,
	                            a.EndDate,
	                            a.Reason 
                            FROM
	                            XYD_Leave_Record a
	                            LEFT JOIN ORG_Employee d ON a.EmplID = d.EmplID 
                                {0} 
                            WHERE 1=1
                            AND a.Status = 'YES'
                                {1}
                            ORDER BY
                                a.StartDate DESC", areaCondition, whereCondition);
                var list = DbUtil.ExecuteSqlCommand(sql, DbUtil.GetLeaveRecord);
                // 记录总页数
                var totalCount = list.Count();
                var results = list.Skip(Page * Size).Take(Size);
                var vos = new List<XYD_Leave_Search_VO>();
                foreach (XYD_Leave_Search item in results)
                {
                    XYD_Leave_Search_VO vo = new XYD_Leave_Search_VO();
                    vo.EmplID = item.EmplID;
                    vo.EmplName = item.EmplName;
                    vo.EmplNO = item.EmplNO;
                    vo.Category = item.Category;
                    vo.Reason = item.Reason;
                    // 获得对应城市工作时间配置
                    var sysConfig = CalendarUtil.GetSysConfigByUser(item.EmplID);
                    if (CalendarUtil.IsDayDate(item.StartDate))
                    {
                        vo.TimeType = "天";
                        vo.StartDate = item.StartDate.ToString("yyyy-MM-dd");
                        vo.EndDate = item.EndDate == null ? string.Empty : item.EndDate.Value.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        vo.TimeType = "小时";
                        vo.StartDate = item.StartDate.ToString("yyyy-MM-dd HH:mm");
                        vo.EndDate = item.EndDate == null ? string.Empty : item.EndDate.Value.ToString("yyyy-MM-dd HH:mm");
                    }
                    if (item.Category == "补打卡")
                    {
                        vo.EndDate = string.Empty;
                    }
                    if (item.Category.Contains("假") || item.Category == "调休")
                    {
                        vo.Hours = CalendarUtil.FillUpToHalfHour(CalendarUtil.GetRealLeaveHours(sysConfig, item.StartDate, item.EndDate.Value));
                    }
                    else
                    {
                        // 外勤和加班
                        vo.Hours = CalendarUtil.FillDownToHalfHour(CalendarUtil.GetRealLeaveHours(sysConfig, item.StartDate, item.EndDate.Value));
                    }
                    vos.Add(vo);
                }
                var totalPage = (int)Math.Ceiling((float)totalCount / Size);

                return ResponseUtil.OK(new
                {
                    results = vos,
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
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 出勤类型
        public ActionResult LeaveCategory()
        {
            var categoryList = new List<string>()
            {
                "事假",
                "病假",
                "年假",
                "婚假",
                "产假",
                "哺乳假",
                "丧假",
                "加班",
                "调休",
                "外勤",
                "补打卡"
            };
            return ResponseUtil.OK(categoryList);
        }
        #endregion

        #region 报销统计类型
        [Authorize]
        public ActionResult ExpenseCategory()
        {
            var categoryList = new List<string>()
            {
                "车辆费用报销",
                "出差费用报销",
                "代付费用报销",
                "付款申请",
                "其他现金支付报销",
                "物品采购费用报销",
                "业务接待费报销"
            };
            return ResponseUtil.OK(categoryList);
        }
        #endregion
    }
}
