using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace XYD.Models
{
    #region 签批记录
    public class XYD_Audit_Record
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 节点
        public string NodeName { get; set; }
        // 签批人
        public string EmplID { get; set; }
        // 申请ID
        public string MessageID { get; set; }
        // 操作
        public string Operation { get; set; }
        // 意见
        public string Opinion { get; set; }
        // 签批时间
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
        public DbSet<XYD_Audit_Record> Audit_Record { get; set; }
    }
    #endregion
}