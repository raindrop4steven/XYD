using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
    }
}