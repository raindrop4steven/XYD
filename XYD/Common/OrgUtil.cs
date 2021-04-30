using Appkiz.Library.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XYD.Models;
using XYD.Common;
using XYD.Entity;

namespace XYD.Common
{
    public class OrgUtil
    {

        static OrgMgr orgMgr = new OrgMgr();

        #region 判断当前用户是否拥有角色
        public static bool CheckRole(string emplID, string roleName)
        {
            OrgMgr orgMgr = new OrgMgr();
            List<Role> roles = orgMgr.FindRoleForEmplID(emplID);
            foreach(var role in roles)
            {
                if (role.RoleName == roleName)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region 检查是否是CEO
        public static bool CheckCEO(string emplId)
        {
            return OrgUtil.CheckRole(emplId, "总经理");
        }
        #endregion

        #region 检查是否是无锡
        public static string GetWorkArea(string emplId)
        {
            if (CheckRole(emplId, DEP_Constants.Role_Name_WuXi))
            {
                return DEP_Constants.System_Config_Area_WX;
            }
            else
            {
                return DEP_Constants.System_Config_Area_SH;
            }
        }
        #endregion

        #region 检查是否是报销专员
        public static bool CheckBaoxiaoUser(string emplId)
        {
            var baoxiaoRole = System.Configuration.ConfigurationManager.AppSettings["BaoXiaoUser"];
            return CheckRole(emplId, baoxiaoRole);
        }
        #endregion

        #region 获取当前部门及子部门所有用户
        public static List<Employee> GetChildrenDeptRecursive(string deptId)
        {
            var users = orgMgr.FindUser(deptId, string.Empty, true, "EmplID", 0, 0);
            var employees = new List<Employee>();
            foreach(var user in users)
            {
                employees.Add(orgMgr.GetEmployee(user.EmplID));
            }
            return employees;
        }
        #endregion

        #region 获取角色中所有用户
        public static List<Employee> GetUsersByRole(string RoleName)
        {
            var sql = string.Format(@"SELECT
	                                    * 
                                    FROM
	                                    ORG_Employee a
	                                    INNER JOIN ORG_EmplRole b ON a.EmplID = b.EmplID
	                                    INNER JOIN ORG_Role c ON b.RoleID = c.RoleID 
	                                    AND c.RoleName = '{0}'", RoleName);
            var roleUsers = orgMgr.FindEmployeeBySQL(sql);
            return roleUsers;
        }
        #endregion

        #region 获取入职月份在查询开始月份后的用户
        public static List<string> GetExceedUsers(DateTime beginDate)
        {
            using(var db = new DefaultConnection())
            {
                var exceedUsers = db.UserCompanyInfo.Where(n => n.EmployeeDate == null ||  n.EmployeeDate.Value > beginDate).Select(n => n.EmplID).ToList();
                return exceedUsers;
            }
        }
        #endregion

        #region 根据地区获取用户工号列表
        public static string GetQueryIdList(string Area, string UserName)
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
            List<string> excludeIds = OrgUtil.GetUsersByRole("部门与工资不同人员").Select(n => n.EmplID).ToList();
            List<string> idList = employees.Where(n => !excludeIds.Contains(n.EmplID)).Select(n => "'" + n.EmplNO + "'").ToList();
            string inClause = "''";
            if (idList.Count > 0)
            {
                inClause = string.Join(",", idList);
            }
            return inClause;
        }
        #endregion

        #region 获取部门与工资地区不同人员的列表
        public static string GetSpecialQueryList(string Area, string UserName)
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
            return inClause;
        }
        #endregion
    }
}