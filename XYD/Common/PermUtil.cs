using Appkiz.Library.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XYD.Common
{
    public class PermUtil
    {
        // 用户权限管理器
        private static OrgMgr orgMgr = new OrgMgr();
        // 常量定义
        private static string UserType = "user";
        private static string RoleType = "role";
        private static string DeptType = "dept";

        /// <summary>
        /// 检测当前用户是否具有特定权限
        /// </summary>
        /// <param name="ModuleCode">模块代码</param>
        /// <param name="PermissionCode">权限代码</param>
        /// <param name="EmplID">用户ID</param>
        /// <returns></returns>
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
    }
}