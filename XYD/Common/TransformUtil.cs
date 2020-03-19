using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XYD.Entity;
using XYD.Models;

namespace XYD.Common
{
    /// <summary>
    /// 解析页面字段，前3个参数固定为：
    /// 用户ID、节点ID、流程ID
    /// </summary>
    public class TransformUtil
    {
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
            catch (Exception e)
            {
                return null;
            }
        }
        #endregion

        #region 解析Feilds中事务编号列表
        public static List<XYD_Cell_Options> GetSerialOptions(string user, string nid, string mid)
        {
            if (OrgUtil.CheckCEO(user))
            {
                return null;
            }
            else
            {
                XYD_Serial serial = WorkflowUtil.GetSourceSerial(mid);
                using (var db = new DefaultConnection())
                {
                    var records = db.SerialRecord.Where(n => n.WorkflowID == serial.FromId && n.Used == false && n.EmplID == user).OrderByDescending(n => n.CreateTime).Select(n => new XYD_Cell_Options() { Value = n.Sn, InterValue = string.Empty }).ToList();
                    return records;
                }
            }
        }
        #endregion

        #region 判断是否为CEO
        public static bool CheckIsCEO(string user, string nid, string mid)
        {
            return !OrgUtil.CheckCEO(user);
        }
        #endregion

        #region 测试event方法
        public static object TestFunc(XYD_Event_Argument eventArgument, string arg1, string arg2, string arg3, string arg4)
        {
            return new
            {
                arguments = eventArgument,
                arg1 = arg1,
                arg2 = arg2,
                arg3 = arg3,
                arg4 = arg4
            };
        }
        #endregion
    }
}