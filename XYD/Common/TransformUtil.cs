using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XYD.Entity;

namespace XYD.Common
{
    public class TransformUtil
    {
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

        public static object TestFunc(string arg1, string arg2, string arg3, string arg4)
        {
            return string.Format("{0},{1},{2},{3}", arg1, arg2, arg3, arg4);
        }

        #region 获取数据库下拉选项
        public static List<XYD_Cell_Options> GetDBOptions(string sql)
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
    }
}