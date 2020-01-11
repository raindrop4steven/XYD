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

    #region 紧急联络人
    public class XYD_Contact
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 用户ID
        public string EmplID { get; set; }
        // 姓名
        public string Name { get; set; }
        // 联系方式
        public string Contact { get; set; }
        // 创建时间
        public DateTime CreateTime { get; set; }
        // 更新时间
        public DateTime UpdateTime { get; set; }
    }
    #endregion

    #region 工作经历
    public class XYD_WorkExperience
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 用户ID
        public string EmplID { get; set; }
        // 公司名称
        public string CompanyName { get; set; }
        // 职位名称
        public string JobName { get; set; }
        // 入职时间
        public DateTime? StartDate { get; set; }
        // 离职时间
        public DateTime? EndDate { get; set; }
        // 工作内容
        public string JobContent { get; set; }
        // 创建时间
        public DateTime CreateTime { get; set; }
        // 更新时间
        public DateTime UpdateTime { get; set; }
    }
    #endregion

    #region 教育经历
    public class XYD_Education
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 用户ID
        public string EmplID { get; set; }
        // 学校
        public string School { get; set; }
        // 入学时间
        public DateTime? StartDate { get; set; }
        // 毕业时间
        public DateTime? EndDate { get; set; }
        // 学历
        public string Level { get; set; }
        // 专业
        public string Major { get; set; }
        // 创建时间
        public DateTime CreateTime { get; set; }
        // 更新时间
        public DateTime UpdateTime { get; set; }
    }
    #endregion

    #region 证书及获奖
    public class XYD_Award
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 用户ID
        public string EmplID { get; set; }
        // 名称
        public string Name { get; set; }
        // 附件
        public string Attachment { get; set; }
        // 创建时间
        public DateTime CreateTime { get; set; }
        // 更新时间
        public DateTime UpdateTime { get; set; }
    }
    #endregion

    #region 附件模型
    public class XYD_Att
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

    #region 固定资产
    public class XYD_Asset
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 资产编号
        public string Sn { get; set; }
        // 资产名称
        public string Name { get; set; }
        // 资产类别
        public int Category { get; set; }
        // 备注
        public string Memo { get; set; }
        // 资产状态：可申领，已申领，已报废
        public string Status { get; set; }
        // 创建时间
        public DateTime CreateTime { get; set; }
        // 更新时间
        public DateTime UpdateTime { get; set; }
    }
    #endregion

    #region 固定资产操作记录
    public class XYD_Asset_Record
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 资产ID
        public int AssetID { get; set; }
        // 操作类别：添加、申请、归还、报废
        public string Operation { get; set; }
        // 使用人员
        public string EmplName { get; set; }
        // 所在部门
        public string DeptName { get; set; }
        // 创建时间
        public DateTime CreateTime { get; set; }
        // 更新时间
        public DateTime UpdateTime { get; set; }
    }
    #endregion

    #region 资产类别
    public class XYD_Asset_Category
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 名字
        public string Name { get; set; }
        // 排序
        public int Order { get; set; }
    }
    #endregion

    #region 系统配置
    public class XYD_System_Config
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 区域：无锡、上海
        public string Area { get; set; }
        // 考勤开始时间 
        public string StartWorkTime { get; set; }
        // 考勤结束时间
        public string EndWorkTime { get; set; }
        // 年假天数
        public int RestDays { get; set; }
        // 出差补贴标准
        public float Allowance { get; set; }
    }
    #endregion

    #region 会议室预约
    public class XYD_MettingBook
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 申请人
        public string EmplID { get; set; }
        // 地区
        public string Area { get; set; }
        // 会议室
        public string MeetingRoom { get; set; }
        // 会议名称
        public string Name { get; set; }
        // 开始时间
        public DateTime StartTime { get; set; }
        // 结束时间
        public DateTime EndTime { get; set; }
        // 是否已批准
        public bool Agreed { get; set; }
        // 创建时间
        public DateTime CreateTime { get; set; }
        // 更新时间
        public DateTime UpdateTime { get; set; }
    }
    #endregion

    #region 请假记录
    public class XYD_Leave_Record
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 请假人
        public string EmplID { get; set; }
        // 事假类别
        public string Category { get; set; }
        // 开始时间
        public DateTime StartDate { get; set; }
        // 结束时间
        public DateTime EndDate { get; set; }
        // 事由
        public string Reason { get; set; }
        // 是否批准
        public bool Approved { get; set; }
        // 创建时间
        public DateTime CreateTime { get; set; }
        // 更新时间
        public DateTime UpdateTime { get; set; }
    }
    #endregion

    #region 轮播图
    public class XYD_Banner
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 图片
        public string Iamge { get; set; }
        // 顺序
        public int order { get; set; }
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
        public DbSet<XYD_Contact> Contact { get; set; }
        public DbSet<XYD_WorkExperience> WorkExperience { get; set; }
        public DbSet<XYD_Education> Education { get; set; }
        public DbSet<XYD_Award> Award { get; set; }
        public DbSet<XYD_Att> Attachment { get; set; }
        public DbSet<XYD_Asset> Asset { get; set; }
        public DbSet<XYD_Asset_Record> AssetRecord { get; set; }
        public DbSet<XYD_Asset_Category> AssetCategory { get; set; }
        public DbSet<XYD_System_Config> SystemConfig { get; set; }
        public DbSet<XYD_Banner> Banner { get; set; }
        public DbSet<XYD_MettingBook> MettingBook { get; set; }
        public DbSet<XYD_Leave_Record> LeaveRecord { get; set; }
    }
    #endregion
}