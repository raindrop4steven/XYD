using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XYD.Entity;

namespace XYD.Common
{
    public class ReflectionUtil
    {
        #region 解析自定义方法参数
        public static XYD_Custom_Func ParseCustomFunc(string originStr)
        {
            var funcStr = originStr.Substring(8, originStr.Length - 9);
            var customFunc = new XYD_Custom_Func();
            var eventArray = funcStr.Split(',').Select(n => n.Trim()).ToList();
            customFunc.ClassName = eventArray[0];
            customFunc.MethodName = eventArray[1];
            customFunc.ArgumentsArray = eventArray.Skip(2).Take(eventArray.Count - 2).Select(n => n.Trim()).ToList();
            return customFunc;
        }
        #endregion

        #region 反射调用
        public static object caller(string myclass, string mymethod, List<object> parameters)
        {
            Assembly.GetEntryAssembly();
            return Type.GetType(myclass)?.GetMethod(mymethod)?.Invoke((object)null, parameters.Cast<object>().ToArray());
        }
        #endregion

        #region 获取FieldValue
        public static dynamic GetFieldValue(string user, string nid, string mid, string customFuncStr)
        {
            XYD_Custom_Func customFunc = ParseCustomFunc(customFuncStr);
            customFunc.ArgumentsArray.Insert(0, user);
            customFunc.ArgumentsArray.Insert(1, nid);
            customFunc.ArgumentsArray.Insert(2, mid);
            return caller(customFunc.ClassName, customFunc.MethodName, customFunc.ArgumentsArray.Cast<object>().ToList());
        }
        #endregion

        #region 解析渲染表单
        public static XYD_Cell_Value ParseCellValue(string emplId, string NodeId, string MessageID, XYD_Cell_Value cellValue)
        {
            PropertyInfo[] properties = typeof(XYD_Cell_Value).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var propertyValue = property.GetValue(cellValue);
                if (propertyValue != null && propertyValue is string && propertyValue.ToString().StartsWith(DEP_Constants.Custom_Func_Header))
                {
                    var resultValue = GetFieldValue(emplId, NodeId, MessageID, propertyValue.ToString());
                    property.SetValue(cellValue, resultValue);
                }
            }
            return cellValue;
        }
        #endregion
    }
}