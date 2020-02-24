using Appkiz.Library.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XYD.Common
{
    public class OrgUtil
    {
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
    }
}