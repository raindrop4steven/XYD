using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using XYD.Entity;

namespace XYD.Common
{
    /// <summary>
    /// 解析页面字段，前3个参数固定为：
    /// 用户ID、节点ID、流程ID
    /// </summary>
    public class TransformUtil
    {
        static OrgMgr orgMgr = new OrgMgr();
        static WorkflowMgr mgr = new WorkflowMgr();

        #region 根据输入对象，转化array
        public static object TransformArray(string inObj)
        {
            JArray jObj = JArray.FromObject(JsonConvert.DeserializeObject(inObj));

            if(jObj.Count == 0)
            {
                return null;
            }
            else
            {
                return jObj;
            }
        }
        #endregion

        #region 解析Fields获取数据库下拉选项
        public static List<XYD_Cell_Options> GetDBOptions(string user, string nid, string mid, string sql)
        {
            try
            {
                var resultOptions = new List<XYD_Cell_Options>();
                var options = DbUtil.ExecuteSqlCommand(sql, DbUtil.GetOptions);
                foreach(XYD_Cell_Options option in options)
                {
                    resultOptions.Add(option);
                }
                return resultOptions;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region 解析Fields获取部门列表
        public static List<XYD_Cell_Options> GetDepts(string user, string nid, string mid)
        {
            var sql = @"select DeptName, DeptID from ORG_Department WHERE DeptDescr != '' ORDER BY DeptDescr";
            var resultOptions = new List<XYD_Cell_Options>();
            var options = DbUtil.ExecuteSqlCommand(sql, DbUtil.GetOptions);
            foreach (XYD_Cell_Options option in options)
            {
                resultOptions.Add(option);
            }
            return resultOptions;
        }
        #endregion
    }
}