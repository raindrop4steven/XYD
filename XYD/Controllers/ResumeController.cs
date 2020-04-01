using Appkiz.Library.Security;
using Appkiz.Library.Security.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Entity;
using XYD.Models;

namespace XYD.Controllers
{
    public class ResumeController : Controller
    {
        // 用户管理
        OrgMgr orgMgr = new OrgMgr();

        #region 用户简历信息
        [Authorize]
        public ActionResult Info(string EmplID)
        {
            Employee employee = null;
            // 用户基本信息
            if (string.IsNullOrEmpty(EmplID))
            {
                employee = (User.Identity as AppkizIdentity).Employee;
            }
            else
            {
                employee = orgMgr.GetEmployee(EmplID);
            }

            using (var db = new DefaultConnection())
            {
                // 用户信息
                var userInfo = db.UserInfo.Where(n => n.EmplID == employee.EmplID).FirstOrDefault();
                // 用户公司信息
                var userCompanyInfo = db.UserCompanyInfo.Where(n => n.EmplID == employee.EmplID).FirstOrDefault();
                // 联系人
                var contacts = db.Contact.Where(n => n.EmplID == employee.EmplID).OrderByDescending(n => n.UpdateTime).ToList();
                // 工作经历
                var experiences = db.WorkExperience.Where(n => n.EmplID == employee.EmplID).OrderByDescending(n => n.StartDate).ToList();
                // 教育经历
                var educations = db.Education.Where(n => n.EmplID == employee.EmplID).OrderByDescending(n => n.StartDate).ToList();
                // 证书情况
                var awards = db.Award.ToList().Where(n => n.EmplID == employee.EmplID).OrderBy(n => n.CreateTime).Select(n => new {
                    ID = n.ID,
                    EmplID = n.EmplID,
                    Name = n.Name,
                    Attachments = db.Attachment.ToList().Where(m => n.Attachment.Split(',').Select(int.Parse).ToList().Contains(m.ID)).Select(m => new {
                        id = m.ID,
                        name = m.Name,
                        url = Url.Action("Download", "Common", new { id = m.ID })
                    })
                }).ToList();

                return ResponseUtil.OK(new {
                    baseInfo = employee,
                    userInfo = userInfo == null ? new XYD_UserInfo() { EmplID = employee.EmplID } : userInfo,
                    userCompanyInfo = userCompanyInfo == null ? new XYD_UserCompanyInfo() { EmplID = employee.EmplID } : userCompanyInfo,
                    contacts = contacts,
                    experiences = experiences,
                    educations = educations,
                    awards = awards
                });
            }
        }
        #endregion

        #region 判断用户是否具有领导权限
        [Authorize]
        public ActionResult CheckLeader()
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                var isLeader = PermUtil.CheckPermission(employee.EmplID, DEP_Constants.Module_Information_Code, DEP_Constants.Perm_Info_Leader);
                return ResponseUtil.OK(new
                {
                    isLeader = isLeader
                });
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 查询工资
        [Authorize]
        public ActionResult QuerySalary(DateTime BeginDate, DateTime EndDate)
        {
            try
            {
                // 参数获取
                var employee = (User.Identity as AppkizIdentity).Employee;

                // 参数校验
                if (string.IsNullOrEmpty(employee.EmplNO))
                {
                    return ResponseUtil.Error("用户工号为空，请先补充相关信息再查询");
                }

                // 构造查询 
                // sql 
                var sql = string.Format(@"SELECT
	                                            F_3 as salary, iYear, iMonth
                                            FROM
	                                            WA_GZData 
                                            WHERE
	                                            cGZGradeNum = '001' 
	                                            AND cPsn_Num = '{0}'
	                                            AND iYear >= {1}
                                                AND iYear <= {2}
	                                            AND iMonth >= {3}
	                                            AND iMonth <= {4}
	                                            ORDER BY iYear, iMonth", employee.EmplNO, BeginDate.Year, EndDate.Year, BeginDate.Month, EndDate.Month);
                // SQL 连接字符串
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["YongYouConnection"].ConnectionString;
                var result = DbUtil.ExecuteSqlCommand(connectionString, sql, DbUtil.GetSalary);
                var salaryList = new List<decimal>();
                // 计算工资
                foreach(XYD_Salary salary in result)
                {
                    salaryList.Add(salary.Salary);
                }
                return ResponseUtil.OK(new
                {
                    result = result,
                    max = salaryList.Count == 0 ? 0 : salaryList.Max(),
                    min = salaryList.Count == 0 ? 0 : salaryList.Min(),
                    avg = salaryList.Count == 0 ? 0 : salaryList.Average()
                });
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 网页端查询工资
        [Authorize]
        public ActionResult QueryDetailSalary(DateTime BeginDate, DateTime EndDate, string UserName, string Area, int Page, int Size)
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
	                                        F_3 
                                        FROM
	                                        WA_GZData a
	                                        LEFT JOIN Department b ON b.cDepCode = a.cDept_Num ";
                if (isLeader)
                {
                    var areaCondition = string.Empty;
                    if (!string.IsNullOrEmpty(Area))
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
                        areaCondition = string.Format(@" INNER JOIN ORG_EmplRole b on a.EmplID = b.EmplID INNER JOIN ORG_Role c on b.RoleID = c.RoleID and c.RoleName = '{0}'", AreaName);
                    }
                    List<Employee> employees = orgMgr.FindEmployeeBySQL(string.Format(@"SELECT
                                                                                            *
                                                                                        FROM
                                                                                            ORG_Employee a
                                                                                            {0}
                                                                                        WHERE
                                                                                            a.EmplName LIKE '%{1}%'
                                                                                            AND a.EmplNO != ''", areaCondition, UserName));
                    List<string> idList = employees.Select(n => "'" + n.EmplNO +"'").ToList();
                    string inClause = string.Join(",", idList);
                    // 构造查询 
                    sql = string.Format(@"SELECT
                                            {0}
                                            WHERE
	                                            cGZGradeNum LIKE '{6}%' 
	                                            AND cPsn_Num in ({1})
	                                            AND iYear >= {2}
                                                AND iYear <= {3}
	                                            AND iMonth >= {4}
	                                            AND iMonth <= {5}
                                                ORDER BY b.cDepCode, iYear, iMonth", selectClause, inClause, BeginDate.Year, EndDate.Year, BeginDate.Month, EndDate.Month, Area);
                }
                else
                {
                    // 参数校验
                    if (string.IsNullOrEmpty(employee.EmplNO))
                    {
                        return ResponseUtil.Error("用户工号为空，请先补充相关信息再查询");
                    }
                    // sql 
                    sql = string.Format(@"SELECT
	                                           {0}
                                            WHERE
	                                            cPsn_Num = '{1}'
	                                            AND iYear >= {2}
                                                AND iYear <= {3}
	                                            AND iMonth >= {4}
	                                            AND iMonth <= {5}
                                                ORDER BY iYear, iMonth", selectClause, employee.EmplNO, BeginDate.Year, EndDate.Year, BeginDate.Month, EndDate.Month);
                }
                // SQL 连接字符串
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["YongYouConnection"].ConnectionString;
                // 记录总页数
                var list = DbUtil.ExecuteSqlCommand(connectionString, sql, DbUtil.GetDetailSalary);
                var totalCount = list.Count();
                var results = list.Skip(Page * Size).Take(Size);
                var totalPage = (int)Math.Ceiling((float)totalCount / Size);

                return ResponseUtil.OK(new
                {
                    results = results,
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
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 地区列表
        [Authorize]
        public ActionResult SalaryAreaList()
        {
            return ResponseUtil.OK(new List<object>{
                new
                {
                    Code = "001",
                    Name = "无锡"
                },
                new
                {
                    Code = "002",
                    Name = "上海"
                }
            });
        }
        #endregion

        #region 检查身份证是否完善和正确
        public ActionResult CheckCredNo(string inputCredNo)
        {
            using (var db = new DefaultConnection())
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                var userInfo = db.UserInfo.Where(n => n.EmplID == employee.EmplID).FirstOrDefault();
                if (userInfo == null)
                {
                    return ResponseUtil.Error("请先补全用户信息");
                }
                else if (string.IsNullOrEmpty(userInfo.CredNo))
                {
                    return ResponseUtil.Error("请先补全身份证信息");
                }
                else if (userInfo.CredNo.Substring(userInfo.CredNo.Length-4) != inputCredNo)
                {
                    return ResponseUtil.Error("身份证后四位不正确");
                }
                else
                {
                    return ResponseUtil.OK("身份证验证通过");
                }
            }
        }
        #endregion

        #region 员工列表
        [Authorize]
        public ActionResult EmployeeList(int Page, int Size, string UserName, string ContractDate)
        {
            try
            {
                var contractDateCondition = string.Empty;
                if (!string.IsNullOrEmpty(ContractDate))
                {
                    contractDateCondition = string.Format(@"AND Convert(varchar,e.ContractDate,120) LIKE '%{0}%'", ContractDate);
                }
                var sql = string.Format(@"SELECT
	                                        ISNULL( a.EmplID, '' ) AS EmplID,
	                                        ISNULL( a.EmplNO, '' ) AS EmplNO,
	                                        ISNULL( a.EmplName, '' ) AS EmplName,
	                                        ISNULL( b.DeptName, '' ) AS DeptName,
	                                        ISNULL( d.PositionName, '' ) AS PositionName,
	                                        e.ContractDate
                                        FROM
	                                        ORG_Employee a
	                                        LEFT JOIN ORG_Department b ON a.DeptID = b.DeptID
	                                        INNER JOIN ORG_EmplDept c ON c.EmplID = a.EmplID
	                                        INNER JOIN ORG_Position d ON d.PositionID = c.PosID 
	                                        LEFT JOIN XYD_UserCompanyInfo e on a.EmplID = e.EmplID
                                        WHERE
	                                        a.EmplEnabled = 1 
	                                        AND a.EmplName LIKE '%{0}%' 
	                                        {1}
                                        ORDER BY
	                                        GlobalSortNo DESC", UserName, contractDateCondition);
                var list = DbUtil.ExecuteSqlCommand(sql, DbUtil.searchEmployee);
                var totalCount = list.Count();
                var results = list.Skip(Page * Size).Take(Size);
                var totalPage = (int)Math.Ceiling((float)totalCount / Size);

                return ResponseUtil.OK(new
                {
                    results = results,
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
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 用户信息修改
        [Authorize]
        public ActionResult AddOrUpdateUserInfo(XYD_UserInfo userInfo)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var info = db.UserInfo.Where(n => n.EmplID == userInfo.EmplID).FirstOrDefault();
                    if (info == null)
                    {
                        info = userInfo;
                        db.UserInfo.Add(info);
                    } else
                    {
                        CommonUtils.CopyProperties<XYD_UserInfo>(userInfo, info);
                    }
                    db.SaveChanges();
                    return ResponseUtil.OK("更改成功");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 用户公司信息修改
        [Authorize]
        public ActionResult AddOrUpdateUserCompany(XYD_UserCompanyInfo userCompanyInfo)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var companyInfo = db.UserCompanyInfo.Where(n => n.EmplID == userCompanyInfo.EmplID).FirstOrDefault();
                    if (companyInfo == null)
                    {
                        companyInfo = userCompanyInfo;
                        db.UserCompanyInfo.Add(companyInfo);
                    }
                    else
                    {
                        CommonUtils.CopyProperties<XYD_UserCompanyInfo>(userCompanyInfo, companyInfo);
                    }
                    db.SaveChanges();
                    return ResponseUtil.OK("更新成功");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region OA用户同步到U8
        public ActionResult SyncPerson()
        {
            try
            {
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["YongYouConnection"].ConnectionString;
                var U8Persons = OrgUtil.GetU8Users();
                var personCodes = U8Persons.Select(n => string.Format("'{0}'", n.cPersonCode)).ToList();
                var inString = string.Join(",", personCodes);
                var sql = string.Format(@"SELECT
	                                            a.EmplNO AS cPersonCode,
	                                            a.EmplName AS cPersonName,
	                                            b.DeptDescr AS cDepCode 
                                            FROM
	                                            ORG_Employee a
	                                            INNER JOIN ORG_Department b ON a.DeptID = b.DeptID 
                                            WHERE
	                                            a.EmplNO != '' 
                                                AND a.EmplEnabled = 1
	                                            AND a.EmplNO NOT IN ( {0});", inString);
                var personList = DbUtil.ExecuteSqlCommand(sql, DbUtil.GetU8Person);
                var insertSqlList = new StringBuilder();
                foreach(XYD_U8_Person person in personList)
                {
                    string insertSql = string.Format(@"INSERT INTO Person (cPersonCode, cPersonName, cDepCode) VALUES ('{0}', '{1}', '{2}');", person.cPersonCode, person.cPersonName, person.cDepCode);
                    insertSqlList.Append(insertSql);
                }
                // 检查是否有更新
                var batchSql = insertSqlList.ToString();
                if (!string.IsNullOrEmpty(batchSql))
                {
                    DbUtil.ExecuteSqlCommand(connectionString, batchSql);
                    return ResponseUtil.OK("同步成功");
                }
                else
                {
                    return ResponseUtil.OK("记录已是最新，无需同步");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 续签
        [Authorize]
        public ActionResult ContinueContract(string EmplID)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var userCompanyInfo = db.UserCompanyInfo.Where(n => n.EmplID == EmplID).FirstOrDefault();
                    if (userCompanyInfo == null)
                    {
                        return ResponseUtil.Error("记录不存在");
                    } else if (userCompanyInfo.ContractDate == null)
                    {
                        return ResponseUtil.Error("请先补全劳动合同日期");
                    } else
                    {
                        userCompanyInfo.ContractDate = userCompanyInfo.ContractDate.Value.AddYears(1);
                        userCompanyInfo.ContinueCount += 1;
                        db.SaveChanges();
                        return ResponseUtil.OK("合同续签成功");
                    }
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