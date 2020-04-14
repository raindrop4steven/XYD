using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
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
        static WorkflowMgr mgr = new WorkflowMgr();
        static SheetMgr sheetMgr = new SheetMgr();
        static OrgMgr orgMgr = new OrgMgr();

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
            // 填充编号
            return EventResult.OK(new XYD_Fields() { Fields = eventArgument.Fields});
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
                Worksheet worksheet = WorkflowUtil.GetWorksheet(mid);
                // 填充编号
                WorkflowUtil.UpdateCell(worksheet, 5, 3, eventArgument.CurrentCellValue.Value, string.Empty);
                // 计算补贴
                CaculateAllowacne(ref worksheet, user, "#C-7-13", "#C-18-14");
                XYD_Fields fields = WorkflowUtil.GetStartFields(user, eventArgument.NodeId, mid);
                return EventResult.OK(fields);
            }
        }
        /// <summary>
        /// 计算补贴
        /// </summary>
        /// <param name="user"></param>
        /// <param name="days"></param>
        public static void CaculateAllowacne(ref Worksheet worksheet, string user, string dayId, string allowanceId)
        {
            if (!OrgUtil.CheckRole(user, "无补贴人员"))
            {
                var dayPos = WorkflowUtil.GetCellPos(dayId);
                var allowancePos = WorkflowUtil.GetCellPos(allowanceId);
                int dayValue = int.Parse(WorkflowUtil.GetCellValue(worksheet, dayPos.row, dayPos.col, 0).ToString());
                var allowance = 65 + (dayValue - 1) * 100;
                WorkflowUtil.UpdateCell(worksheet, allowancePos.row, allowancePos.col, allowance.ToString(), string.Empty);
            }
        }

        /// <summary>
        /// 判断住宿费是否超过预算
        /// </summary>
        /// <param name="user"></param>
        /// <param name="eventArguments"></param>
        /// <param name="city"></param>
        /// <param name="hotelFee"></param>
        /// <returns></returns>
        public static object TR_01_HotelFeeUpdate(string user, XYD_Event_Argument eventArguments, string city, string dayStr, string hotelFee)
        {
            var mid = eventArguments.MessageId;
            int day = int.Parse(dayStr);
            if (!OrgUtil.CheckCEO(user))
            {
                int standard = WorkflowUtil.GetHotelStandard(mid, city, day * 24);
                if (float.Parse(hotelFee) > standard)
                {
                    throw new Exception("住宿费用超过补贴标准");
                }
            }
            return EventResult.OK("费用检测通过");
        }
        #endregion

        #region 物品采购费用报销
        /// <summary>
        /// 事务编号更新事件
        /// </summary>
        /// <param name="eventArgument"></param>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        public static object GB_01_SerialNoUpdate(string user, XYD_Event_Argument eventArgument, string serialNo)
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
                Worksheet worksheet = WorkflowUtil.GetWorksheet(mid);
                // 填充编号
                WorkflowUtil.UpdateCell(worksheet, 4, 3, eventArgument.CurrentCellValue.Value, string.Empty);
                
                XYD_Fields fields = WorkflowUtil.GetStartFields(user, eventArgument.NodeId, mid);
                return EventResult.OK(fields);
            }
        }
        #endregion
    }
}