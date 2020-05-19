using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using XYD.Entity;

namespace XYD.Common
{
    public class CommonUtils
    {
        #region 某天开始时间 00:00:00
        public static DateTime StartOfDay(DateTime theDate)
        {
            return theDate.Date;
        }
        #endregion

        #region 某天结束时间 23:59:59
        public static DateTime EndOfDay(DateTime theDate)
        {
            return theDate.Date.AddDays(1).AddTicks(-1);
        }
        #endregion

        #region 周里面第一天
        public static DateTime FirstDayOfWeek(DateTime date)
        {
            CultureInfo ci = new CultureInfo("zh-Hans");
            ci.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Monday;
            DayOfWeek fdow = ci.DateTimeFormat.FirstDayOfWeek;
            int offset = fdow - date.DayOfWeek;
            DateTime fdowDate = date.AddDays(offset);
            return fdowDate;
        }
        #endregion

        #region 周里面最后一天
        public static DateTime LastDayOfWeek(DateTime date)
        {
            DateTime ldowDate = FirstDayOfWeek(date).AddDays(6);
            return ldowDate;
        }
        #endregion

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
            return Type.GetType(myclass).GetMethod(mymethod).Invoke((object)null, parameters.Cast<object>().ToArray());
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

        #region 填充CellValue
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

        #region 复制对象属性
        public static void CopyProperties<T>(object src, object dest)
        {
            Type t = src.GetType();
            PropertyInfo[] properties = t.GetProperties();

            foreach (PropertyInfo pi in properties)
            {
                if (pi.CanWrite)
                {
                    pi.SetValue(dest, pi.GetValue(src, null), null);
                }
            }
        }
        #endregion
    }
}