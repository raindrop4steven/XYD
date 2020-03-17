using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XYD.Entity;

namespace XYD.Common
{
    public class CellEvents
    {
        #region 测试方法
        public static object TestFunc(XYD_Event_Argument eventArgument, string arg1, string arg2, string arg3, string arg4)
        {
            return new
            {
                arguments = eventArgument,
                arg1 = arg1,
                arg2 = arg2,
                arg3 = arg3,
                arg4 = arg4
            };
        }
        #endregion

        #region 备用金申请
        public static object BM_01_ApplyTypeUpdate(XYD_Event_Argument eventArgument, string applyType)
        {
            XYD_Cell_Value cellValue;
            var lastDayOfYear = string.Empty;
            if (applyType == "长期备用")
            {
                var date = DateTime.Now;
                lastDayOfYear = string.Format("{0}-12-31", date.Year);
                cellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 5, 9);
                cellValue.Value = lastDayOfYear;
                cellValue.CanEdit = false;
            }
            else
            {
                cellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 5, 9);
                cellValue.Value = string.Empty;
                cellValue.CanEdit = true;
            }
            WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, cellValue);
            return new
            {
                refresh = true,
                fields = eventArgument.Fields
            };
        }
        #endregion
    }
}