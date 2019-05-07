using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeptOA.Common
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
    }
}