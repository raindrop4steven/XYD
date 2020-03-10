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
    }
}