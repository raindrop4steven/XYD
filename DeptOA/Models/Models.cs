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

    #region 公文预警

    public class DEP_MessageAlarm
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 消息ID
        public string MessageID { get; set; }
        // 预警日期
        public DateTime? AlarmDate { get; set; }
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
        // 公文预警
        public DbSet<DEP_MessageAlarm> MessageAlarm { get; set; }
    }
    #endregion
}