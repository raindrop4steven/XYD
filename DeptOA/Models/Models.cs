using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace DeptOA.Models
{
    #region 收藏公文
    public class DEP_FavoriteMessage
    {
        [Key]
        public int ID { get; set; }
        // 消息ID
        public string MessageID { get; set; }
        // 收藏用户
        public string EmplID { get; set; }
        // 收藏时间
        public DateTime CreateTime { get; set; }
    }
    #endregion

    #region 新意见
    public class DEP_Opinion
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 用户ID
        public string EmplID { get; set; }
        // 消息ID
        public string MessageID { get; set; }
        // 节点ID
        public string NodeKey { get; set; }
        // 意见内容
        public string Opinion { get; set; }
        // 意见排序（同一个用户的排序）
        public int order { get; set; }
        // 创建时间
        public DateTime? CreateTime { get; set; }
        // 更新时间
        public DateTime? UpdatedTime { get; set; }
    }
    #endregion

    #region 附件模型
    public class DEP_Att
    {
        // 附件ID
        [Key]
        public int ID { get; set; }
        // 名称
        public string Name { get; set; }
        // 路径
        public string Path { get; set; }
    }
    #endregion

    #region 公文预警

    public class DEP_MessageAlarm
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 消息ID
        public string MessageID { get; set; }
        // Hangfire ID
        public string JobID { get; set; }
        // 预警日期
        public DateTime? AlarmDate { get; set; }
    }
    #endregion

    #region 父流程与子流程关系
    public class DEP_SubflowRelation
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 父流程ID
        public string OriginMessageID { get; set; }
        // 子流程ID
        public string SubflowMessageID { get; set; }
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
        // 收藏公文
        public DbSet<DEP_FavoriteMessage> FavoriteMessage { get; set; }
        // 修改意见
        public DbSet<DEP_Opinion> Opinion { get; set; }
        // 附件
        public DbSet<DEP_Att> Att { get; set; }
        // 公文预警
        public DbSet<DEP_MessageAlarm> MessageAlarm { get; set; }
        // 父流程与子流程关系
        public DbSet<DEP_SubflowRelation> SubflowRelation { get; set; }
    }
    #endregion
}