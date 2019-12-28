using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

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
}