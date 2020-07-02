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
            if (applyType == "长期备用")
            {
                var date = DateTime.Now;
                cellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 5, 9);
                cellValue.Value = string.Format("{0}-12-31", date.Year);
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
            XYD_Fields fields = WorkflowUtil.GetStartFields(user, eventArgument.NodeId, eventArgument.MessageId);
            return EventResult.OK(new XYD_Fields() { Fields = eventArgument.Fields, Operations = fields.Operations});
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
                if (sn.Contains(" "))
                {
                    var snArray = sn.Split(' ');
                    user = orgMgr.FindEmployee("EmplName=@EmplName", new System.Collections.Hashtable()
                          {
                            {
                              "@EmplName",
                              snArray[0]
                            }
                          }, string.Empty, 0, 1).FirstOrDefault().EmplID;
                    sn = snArray[1];
                }
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
                if (sn.Contains(" "))
                {
                    var snArray = sn.Split(' ');
                    user = orgMgr.FindEmployee("EmplName=@EmplName", new System.Collections.Hashtable()
                          {
                            {
                              "@EmplName",
                              snArray[0]
                            }
                          }, string.Empty, 0, 1).FirstOrDefault().EmplID;
                    sn = snArray[1];
                }
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

        #region 出勤申请
        /// <summary>
        /// 日期控件类型切换
        /// </summary>
        /// <param name="user"></param>
        /// <param name="eventArgument"></param>
        /// <param name="dateType"></param>
        /// <returns></returns>
        public static object CQ_01_SelectTypeUpdate(string user, XYD_Event_Argument eventArgument, string dateType)
        {
            var mid = eventArgument.MessageId;
            Worksheet worksheet = WorkflowUtil.GetWorksheet(mid);
            var selecTypeValueCell = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 6, 7);
            selecTypeValueCell.Value = eventArgument.CurrentCellValue.Value;
            WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, selecTypeValueCell);

            XYD_Fields fields = WorkflowUtil.GetStartFields(user, eventArgument.NodeId, mid);

            if (dateType == "小时")
            {
                // 开始时间
                var startTimeCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 7, 3);
                startTimeCellValue.Type = 4;
                startTimeCellValue.Value = "";
                // 结束时间
                var endTimeCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 7, 7);
                endTimeCellValue.Type = 4;
                endTimeCellValue.Value = "";
                // 更新表单中选到小时
                WorkflowUtil.UpdateCell(worksheet, 7, 9, "小时数", "");
                var deltaHourCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 7, 11);
                deltaHourCellValue.Title = "小时数";
                deltaHourCellValue.Value = "";
                // 更新Cell
                WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, startTimeCellValue);
                WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, endTimeCellValue);
                WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, deltaHourCellValue);
                // 更新计算公式
                fields.Operations.FirstOrDefault().Type = "hour";
            }
            else
            {
                // 开始时间
                var startTimeCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 7, 3);
                startTimeCellValue.Type = 2;
                startTimeCellValue.Value = "";
                // 结束时间
                var endTimeCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 7, 7);
                endTimeCellValue.Type = 2;
                endTimeCellValue.Value = "";
                // 更新表单中选到小时
                WorkflowUtil.UpdateCell(worksheet, 7, 9, "天数", "");
                var deltaHourCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 7, 11);
                deltaHourCellValue.Title = "天数";
                deltaHourCellValue.Value = "";
                // 更新Cell
                WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, startTimeCellValue);
                WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, endTimeCellValue);
                WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, deltaHourCellValue);
                // 更新计算公式
                fields.Operations.FirstOrDefault().Type = "date";
            }
            fields.Fields = eventArgument.Fields;
            return EventResult.OK(fields);
        }
        
        /// <summary>
        /// 出勤类别切换
        /// </summary>
        /// <param name="user"></param>
        /// <param name="eventArgument"></param>
        /// <param name="dateType"></param>
        /// <returns></returns>
        public static object CQ_02_CategoryUpdate(string user, XYD_Event_Argument eventArgument, string category)
        {
            var mid = eventArgument.MessageId;
            Worksheet worksheet = WorkflowUtil.GetWorksheet(mid);
            WorkflowUtil.UpdateCell(worksheet, 6, 3, eventArgument.CurrentCellValue.Value, string.Empty);
            var categoryValueCell = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 6, 3);
            categoryValueCell.Value = eventArgument.CurrentCellValue.Value;
            WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, categoryValueCell);
            XYD_Fields fields = WorkflowUtil.GetStartFields(user, eventArgument.NodeId, mid);

            if (category == "年假" || category == "婚假" || category == "产假" || category == "丧假")
            {
                // 选到日期
                // 控件类别
                var dateTimeTypeCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 6, 7);
                dateTimeTypeCellValue.CanEdit = false;
                dateTimeTypeCellValue.Value = "天";
                // 开始时间
                var startTimeCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 7, 3);
                startTimeCellValue.Type = 2;
                startTimeCellValue.Value = "";
                // 结束时间
                var endTimeCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 7, 7);
                endTimeCellValue.Type = 2;
                endTimeCellValue.Value = "";
                // 更新表单中选到小时
                WorkflowUtil.UpdateCell(worksheet, 7, 9, "天数", "");
                var deltaHourCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 7, 11);
                deltaHourCellValue.Title = "天数";
                deltaHourCellValue.Value = "";
                // 更新Cell
                WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, dateTimeTypeCellValue);
                WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, startTimeCellValue);
                WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, endTimeCellValue);
                WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, deltaHourCellValue);
                // 更新计算公式
                fields.Operations.FirstOrDefault().Type = "date";
            }
            else if (category == "哺乳假")
            {
                // 选到小时
                // 控件类别
                var dateTimeTypeCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 6, 7);
                dateTimeTypeCellValue.CanEdit = false;
                dateTimeTypeCellValue.Value = "小时";
                // 开始时间
                var startTimeCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 7, 3);
                startTimeCellValue.Type = 4;
                startTimeCellValue.Value = "";
                // 结束时间
                var endTimeCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 7, 7);
                endTimeCellValue.Type = 4;
                endTimeCellValue.Value = "";
                // 更新表单中选到小时
                WorkflowUtil.UpdateCell(worksheet, 7, 9, "小时数", "");
                var deltaHourCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 7, 11);
                deltaHourCellValue.Title = "小时数";
                deltaHourCellValue.Value = "";
                // 更新Cell
                WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, dateTimeTypeCellValue);
                WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, startTimeCellValue);
                WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, endTimeCellValue);
                WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, deltaHourCellValue);
                // 更新计算公式
                fields.Operations.FirstOrDefault().Type = "hour";
            }
            else if (category == "补打卡")
            {
                //$('#C-6-7').text('小时');
                //SaveCellValue($('#C-6-7'), '小时', '');
                //SetReadonlyCells(['#C-6-7', '#C-7-7']);
                //dateSelectTypeUpdate('小时');
                // 选到小时
                // 控件类别
                var dateTimeTypeCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 6, 7);
                dateTimeTypeCellValue.CanEdit = false;
                dateTimeTypeCellValue.Value = "小时";
                // 开始时间
                var startTimeCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 7, 3);
                startTimeCellValue.Type = 4;
                startTimeCellValue.Value = "";
                // 结束时间
                var endTimeCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 7, 7);
                endTimeCellValue.Type = 4;
                endTimeCellValue.Value = "";
                endTimeCellValue.Required = false;
                endTimeCellValue.CanEdit = false;
                // 更新表单中选到小时
                WorkflowUtil.UpdateCell(worksheet, 7, 9, "小时数", "");
                var deltaHourCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 7, 11);
                deltaHourCellValue.Title = "小时数";
                deltaHourCellValue.Value = "";
                deltaHourCellValue.Required = false;
                deltaHourCellValue.CanEdit = false;
            }
            else
            {
                // 均可
                // 控件类别
                var dateTimeTypeCellValue = WorkflowUtil.GetFieldsCellValue(eventArgument.Fields, 6, 7);
                dateTimeTypeCellValue.CanEdit = true;
                dateTimeTypeCellValue.Value = "";
                WorkflowUtil.UpdateFieldsCellValue(eventArgument.Fields, dateTimeTypeCellValue);
            }
            fields.Fields = eventArgument.Fields;
            return EventResult.OK(fields);
        }
        #endregion
    }
}