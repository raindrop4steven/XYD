using System;

namespace XYD.Entity
{
    /// <summary>
    /// 分页查询模型
    /// </summary>
    public class BaseQueryInfo
    {
        /// <summary>
        /// 排序列名
        /// </summary>
        public string SortColumn { get; set; }
        /// <summary>
        /// 排序asc顺序，desc逆序
        /// </summary>
        public string SortDirection { get; set; }

        /// <summary>
        /// 分页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// 查询条件(文本框输入的模糊查询条件)
        /// </summary>
        public string QueryCondition { get; set; }

        /// <summary>
        /// 公文类型
        /// </summary>
        public string WorkFlowId { get; set; }
    }
    
    public class QueryInfo : BaseQueryInfo
    {
        /// <summary>
        /// 公文标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 文号
        /// </summary>
        public string DocumentNumber { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string DocumentUnit { get; set; }
        /// <summary>
        /// 开始收/发文日期
        /// </summary>
        public DateTime? StartClosedOrHairTime { get; set; }
        /// <summary>
        /// 结束收/发文日期
        /// </summary>
        public DateTime? EndClosedOrHairTime { get; set; }
        /// <summary>
        /// 序列编号
        /// </summary>
        public string SequenceNumber { get; set; }
        /// <summary>
        /// 序列名称
        /// </summary>
        public string SequenceName { get; set; }
        /// <summary>
        /// 开始发起时间
        /// </summary>
        public DateTime? StartCreatedTime { get; set; }
        /// <summary>
        /// 结束发起时间
        /// </summary>
        public DateTime? EndCreatedTime { get; set; }
        /// <summary>
        /// 开始终止时间
        /// </summary>
        public DateTime? StartEndTime { get; set; }
        /// <summary>
        /// 结束终止时间
        /// </summary>
        public DateTime? EndEndTime { get; set; }
        /// <summary>
        /// 开始接收时间
        /// </summary>
        public DateTime? StartReceiveTime { get; set; }
        /// <summary>
        /// 结束接收时间
        /// </summary>
        public DateTime? EndReceiveTime { get; set; }
        /// <summary>
        /// 发起人
        /// </summary>
        public string MessageIssuedBy { get; set; }
    }
    
    public class WorkflowQuery : BaseQueryInfo
    {
        /// <summary>
        /// 流程状态  流程状态：0：草稿 1：运行中 2：已完成 3：终止信息
        /// </summary>
        public int? MessageStatus { get; set; }
        /// <summary>
        /// 开始收/发文日期
        /// </summary>
        public DateTime? StartClosedOrHairTime { get; set; }
        /// <summary>
        /// 结束收/发文日期
        /// </summary>
        public DateTime? EndClosedOrHairTime { get; set; }

        /// <summary>
        /// 文号
        /// </summary>
        public string DocumentNumber { get; set; }

        /// <summary>
        /// 来文单位
        /// </summary>
        public string DocumentUnit { get; set; }
        /// <summary>
        /// 序列编号
        /// </summary>
        public string SequenceNumber { get; set; }
        /// <summary>
        /// 序列名称
        /// </summary>
        public string SequenceName { get; set; }

        /// <summary>
        /// 发起人Id
        /// </summary>
        public string MessageIssuedBy { get; set; }
        /// <summary>
        /// 发起人部门Id
        /// </summary>
        public string MessageIssuedDept { get; set; }

    }
}