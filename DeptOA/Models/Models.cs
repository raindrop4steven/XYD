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

    #region 数据库上下文
    /// <summary>
    /// 科室资金类别
    /// </summary>
    public class DefaultConnection : DbContext
    {
        // 收藏公文
        public DbSet<DEP_FavoriteMessage> FavoriteMessage { get; set; }
    }
    #endregion
}