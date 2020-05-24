using Appkiz.Library.Common;
using System;
using System.Web.Mvc;

namespace XYD.Common
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class LogExceptionAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                var ex = filterContext.Exception;
                var controllerName = (string)filterContext.RouteData.Values["controller"];
                var actionName = (string)filterContext.RouteData.Values["action"];
                var errorMessage = ex.Message + "\r\n" + ex.InnerException?.Message;
                var logMessage = $"在执行 controller[{controllerName}] 的 action[{actionName}] 时产生异常:{errorMessage}";

                LogService.WriteLog("XYD", logMessage, "Action", LogService.LogLevel.Error);
            }

            if (filterContext.Result is JsonResult)
            {
                //当结果为json时，设置异常已处理
                filterContext.ExceptionHandled = true;
            }
            else
            {
                //否则调用原始设置
                filterContext.Result = ResponseUtil.Error(filterContext.Exception.Message);
                filterContext.ExceptionHandled = true;
                base.OnException(filterContext);
            }
        }
    }
}