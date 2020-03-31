using Appkiz.Library.Common;
using System.Web;
using System.Web.Mvc;
using XYD.Common;

namespace XYD
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new LogExceptionAttribute());
        }
    }
}
