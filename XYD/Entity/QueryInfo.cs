using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace XYD.Entity
{
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
    }
}