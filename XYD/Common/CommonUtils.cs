using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XYD.Common
{
    public class CommonUtils
    {
        public static DateTime StartOfDay(DateTime theDate)
        {
            return theDate.Date;
        }

        public static DateTime EndOfDay(DateTime theDate)
        {
            return theDate.Date.AddDays(1).AddTicks(-1);
        }
    }
}