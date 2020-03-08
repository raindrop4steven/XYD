﻿using System;
using System.Collections.Generic;
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
        public static object caller(string myclass, string mymethod, List<string> parameters)
        {
            Assembly.GetEntryAssembly();
            return Type.GetType(myclass).GetMethod(mymethod).Invoke((object)null, parameters.Cast<object>().ToArray());
        }
        #endregion
    }
}