using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using System;
using System.Linq;
using XYD.Entity;

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
            XYD_Fields fields = WorkflowUtil.GetWorkflowFields(user, eventArgument.NodeId, eventArgument.MessageId);
            return EventResult.OK(new XYD_Fields() { Fields = eventArgument.Fields, Operations = fields.Operations});
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

            XYD_Fields fields = WorkflowUtil.GetWorkflowFields(user, eventArgument.NodeId, mid);

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
            XYD_Fields fields = WorkflowUtil.GetWorkflowFields(user, eventArgument.NodeId, mid);

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