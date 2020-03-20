using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XYD.Entity;
using XYD.Models;

namespace XYD.Common
{
    /// <summary>
    /// Cell变更事件方法：
    /// 1. 语法：
    ///     $:XYD_Event_Argument 整体
    ///     #C-5-3: 将会取C-5-3的值作为参数
    ///     普通字符串：直接传入
    /// </summary>
    public class CellEvents
    {
        #region 测试方法
        public static object TestFunc(string user, XYD_Event_Argument eventArgument, string arg1, string arg2, string arg3, string arg4)
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
        public static object BM_01_ApplyTypeUpdate(string user, XYD_Event_Argument eventArgument, string applyType)
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

        #region 差旅费用报销单
        /// <summary>
        /// 事务编号更新事件
        /// </summary>
        /// <param name="eventArgument"></param>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        public static object TR_01_SerialNoUpdate(string user, XYD_Event_Argument eventArgument, string serialNo)
        {
            var mid = eventArgument.MessageId;
            var sn = eventArgument.CurrentCellValue.Value;
            XYD_Serial serial = WorkflowUtil.GetSourceSerial(mid);
            // 设置编号已使用
            using (var db = new DefaultConnection())
            {
                var record = db.SerialRecord.Where(n => n.WorkflowID == serial.FromId && n.Used == false && n.EmplID == user && n.Sn == sn).FirstOrDefault();
                if (record == null)
                {
                    throw new Exception("没有找到对应申请记录");
                }
                WorkflowUtil.MappingBetweenFlows(record.MessageID, mid, serial.MappingOut);
                XYD_Fields fields = WorkflowUtil.GetStartFields(user, eventArgument.NodeId, mid);
                return new
                {
                    refresh = true,
                    fields = fields
                };
            }
        }
        #endregion
    }
}