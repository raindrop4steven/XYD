using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace XYD.Models
{
    #region 事物编号序号表
    public class XYD_Serial_No
    {
        // ID
        public string ID { get; set; }
        // 名称
        public string Name { get; set; }
        // 年
        public int Year { get; set; }
        // 编号最大值
        public int Number { get; set; }
    }
    #endregion

    #region 事务编号记录表
    public class XYD_Serial_Record
    {
        // ID
        public string ID { get; set; }
        // 流程ID
        public string MessageID { get; set; }
        // 用户ID
        public string EmplID { get; set; }
        // 模版ID
        public string WorkflowID { get; set; }
        // 对应编号
        public string Sn { get; set; }
        // 是否使用了
        public bool Used { get; set; }
        // 创建时间
        public DateTime CreateTime { get; set; }
        // 更新时间
        public DateTime UpdateTime { get; set; }
    }
    #endregion

    #region 数据库上下文
    /// <summary>
    /// 科室资金类别
    /// </summary>
    public class DefaultConnection : DbContext
    {
        public DbSet<XYD_Serial_No> SerialNo { get; set; }
        public DbSet<XYD_Serial_Record> SerialRecord { get; set; }
    }
    #endregion
}