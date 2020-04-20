using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
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
            catch (Exception e)
            {
                return null;
            }
        }
        #endregion

        #region 解析Feidls获取司机列表
        public static List<XYD_Cell_Options> GetDrivers(string user, string nid, string mid)
        {
            var sql = @"SELECT a.EmplName, a.EmplID from ORG_Employee a INNER JOIN ORG_EmplDept b on a.EmplID = b.EmplID INNER JOIN ORG_Position c on b.PosID = c.PositionID and c.PositionName = '司机'";
            var resultOptions = new List<XYD_Cell_Options>();
            var options = DbUtil.ExecuteSqlCommand(sql, DbUtil.GetOptions);
            foreach (XYD_Cell_Options option in options)
            {
                resultOptions.Add(option);
            }
            return resultOptions;
        }
        #endregion

        #region 解析Fields获取供应商列表
        public static List<XYD_Cell_Options> GetVendors(string user, string nid, string mid)
        {
            var sql = @"select Name, Code from XYD_Vendor ORDER BY Code";
            var resultOptions = new List<XYD_Cell_Options>();
            var options = DbUtil.ExecuteSqlCommand(sql, DbUtil.GetOptions);
            foreach (XYD_Cell_Options option in options)
            {
                resultOptions.Add(option);
            }
            return resultOptions;
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
                    bool isBaoxiaoRole = OrgUtil.CheckBaoxiaoUser(user);
                    var query = db.SerialRecord.Where(n => n.WorkflowID == serial.FromId && n.Used == false);
                    if (!isBaoxiaoRole)
                    {
                        query = query.Where(n => n.EmplID == user);
                    }
                    var records = query.OrderByDescending(n => n.CreateTime).ToList().Where(m => mgr.GetMessage(m.MessageID).MessageStatus == 2);
                    if (isBaoxiaoRole)
                    {
                        foreach (var record in records)
                        {
                            var thisUser = orgMgr.GetEmployee(record.EmplID).EmplName;
                            var Sn = string.Format("{0} {1}", thisUser, record.Sn);
                            record.Sn = Sn;
                        }
                    }
                    return records.Select(n => new XYD_Cell_Options() { Value = n.Sn, InterValue = string.Empty }).ToList();
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