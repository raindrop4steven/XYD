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

        #region 获得U8用户表数据
        public static Dictionary<string, string> GetU8Person()
        {
            var u8PersonDict = new Dictionary<string, string>();
            var sql = @"SELECT cPersonCode, cPersonName, cDepCode from Person";
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["YongYouConnection"].ConnectionString;
            var result = DbUtil.ExecuteSqlCommand(connectionString, sql, DbUtil.GetU8Person);
            foreach(XYD_U8_Person person in result)
            {
                u8PersonDict.Add(person.cPersonCode, person.cDepCode);
            }
            return u8PersonDict;
        }
        #endregion

        #region 获得U8用户
        public static List<XYD_U8_Person> GetU8Users()
        {
            var u8PersonDict = new Dictionary<string, string>();
            var sql = @"SELECT cPersonCode, cPersonName, cDepCode from Person";
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["YongYouConnection"].ConnectionString;
            var result = DbUtil.ExecuteSqlCommand(connectionString, sql, DbUtil.GetU8Person);
            List<XYD_U8_Person> persons = new List<XYD_U8_Person>();
            foreach(XYD_U8_Person person in result)
            {
                persons.Add(person);
            }
            return persons;
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
    }
}