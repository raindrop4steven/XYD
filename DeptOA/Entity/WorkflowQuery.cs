using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeptOA.Entity
{
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