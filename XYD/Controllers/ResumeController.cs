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
                // 联系人
                var contacts = db.Contact.Where(n => n.EmplID == employee.EmplID).OrderByDescending(n => n.UpdateTime).ToList();
                // 工作经历
                var experiences = db.WorkExperience.Where(n => n.EmplID == employee.EmplID).OrderByDescending(n => n.StartDate).ToList();
                // 教育经历
                var educations = db.Education.Where(n => n.EmplID == employee.EmplID).OrderByDescending(n => n.StartDate).ToList();
                // 证书情况
                var awards = db.Award.ToList().Where(n => n.EmplID == employee.EmplID).OrderBy(n => n.CreateTime).Select(n => new {
                    ID = n.ID,
                    Name = n.Name,
                    Attachments = db.Attachment.ToList().Where(m => n.Attachment.Split(',').Select(int.Parse).ToList().Contains(m.ID)).Select(m => new {
                        id = m.ID,
                        name = m.Name,
                        url = Url.Action("Download", "Common", new { id = m.ID })
                    })
                }).ToList();
                
                return ResponseUtil.OK(new {
                    baseInfo = employee,
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
        public ActionResult QueryDetailSalary(DateTime BeginDate, DateTime EndDate, string UserName, int Page, int Size)
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
                    List<Employee> employees = orgMgr.FindEmployeeBySQL(string.Format("select * from ORG_Employee where EmplName like '%{0}%' and EmplNO != ''", UserName));
                    List<string> idList = employees.Select(n => "'" + n.EmplNO +"'").ToList();
                    string inClause = string.Join(",", idList);
                    // 构造查询 
                    sql = string.Format(@"SELECT
                                            {0}
                                            WHERE
	                                            cGZGradeNum = '001' 
	                                            AND cPsn_Num in ({1})
	                                            AND iYear >= {2}
                                                AND iYear <= {3}
	                                            AND iMonth >= {4}
	                                            AND iMonth <= {5}
                                                ORDER BY b.cDepCode, iYear, iMonth", selectClause, inClause, BeginDate.Year, EndDate.Year, BeginDate.Month, EndDate.Month);
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
	                                            cGZGradeNum = '001' 
	                                            AND cPsn_Num = '{1}'
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

        #region 员工列表
        [Authorize]
        public ActionResult EmployeeList(int Page, int Size, string UserName)
        {
            try
            {
                var sql = string.Format(@"SELECT
                                                ISNULL(a.EmplID, ''),
	                                            ISNULL(a.EmplNO, ''),
	                                            ISNULL(a.EmplName, ''),
	                                            ISNULL(b.DeptName, ''),
	                                            ISNULL(d.PositionName, '')
                                            FROM
	                                            ORG_Employee a
	                                            LEFT JOIN ORG_Department b ON a.DeptID = b.DeptID
	                                            INNER JOIN ORG_EmplDept c ON c.EmplID = a.EmplID
	                                            INNER JOIN ORG_Position d ON d.PositionID = c.PosID 
                                            WHERE
	                                            a.EmplEnabled = 1 
	                                            AND a.EmplName LIKE '%{0}%' 
                                            ORDER BY
	                                            GlobalSortNo DESC", UserName);
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
    }
}