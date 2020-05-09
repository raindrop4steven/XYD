using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XYD.Common
{
    public class ScheduleUtil
    {
        #region 定时任务列表
        public static void ScheduleGlobalUpdate()
        {
            // 除了第一个季度的开始，其他季度都需要将上一个季度余额移动到下一个季度
            RecurringJob.AddOrUpdate(() => RestUsedYearDays(), "59 23 31 12 *");
        }
        #endregion

        #region 年底已使用年假清零
        public static void RestUsedYearDays()
        {
            var sql = @"UPDATE XYD_UserCompanyInfo SET UsedRestDays = 0";
            DbUtil.ExecuteSqlCommand(sql);
        }
        #endregion
    }
}