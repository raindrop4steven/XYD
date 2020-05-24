using Appkiz.Library.Security;
using System.Collections.Generic;

namespace XYD.Common
{
    public class OrgUtil
    {

        static OrgMgr orgMgr = new OrgMgr();
        // 常量定义
        private static string UserType = "user";
        private static string RoleType = "role";
        private static string DeptType = "dept";

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

        #region 判断用户是否具有模块权限
        public static bool CheckPermission(string EmplID, string ModuleCode, string PermissionCode)
        {
            // 权限对应于3种类型：用户ID，用户角色，用户部门
            // 参数校验
            if (string.IsNullOrEmpty(PermissionCode) || string.IsNullOrEmpty(EmplID))
            {
                return false;
            }

            // 获取当前用户
            User CurrentUser = orgMgr.GetUserByEmplID(EmplID);
            if (CurrentUser == null)
            {
                return false;
            }
            // 获得用户部门
            string DeptId = CurrentUser.Employee.DeptID;
            // 获取用户的角色
            List<Role> CurrentRoles = orgMgr.FindRoleForEmplID(EmplID);

            // 检测用户是否有权限
            bool UserHasPermission = orgMgr.VerifyPermission(ModuleCode, EmplID, UserType, PermissionCode);
            // 检测部门是否有权限
            bool DeptHasPermission = orgMgr.VerifyPermission(ModuleCode, DeptId, DeptType, PermissionCode);
            // 检测角色是否有权限
            bool RoleHasPermission = false;
            foreach (var role in CurrentRoles)
            {
                RoleHasPermission = orgMgr.VerifyPermission(ModuleCode, role.RoleID, RoleType, PermissionCode);
                if (RoleHasPermission == true)
                    break;
            }
            return UserHasPermission || DeptHasPermission || RoleHasPermission;
        }
        #endregion
    }
}