﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;

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

    #region 用户其他信息
    public class XYD_UserInfo
    {
        [Key]
        public int ID { get; set; }
        // 用户ID
        public string EmplID { get; set; }
        // 籍贯
        public string BirthPlace { get; set; }
        // 民族
        public string Nation { get; set; }
        // 最高文化程度
        public string TopDegree { get; set; }
        // 婚姻状况
        public bool Marriage { get; set; }
        // 身份证号
        public string CredNo { get; set; }
        // 护照号
        public string PassportNo { get; set; }
        // 港澳通行证号
        public string ExitEntryNo { get; set; }
        // 门禁卡号
        public string DoorNo { get; set; }
        // 户籍所在地
        public string Residence { get; set; }
        // 现居地址
        public string CurrentAddress { get; set; }
    }
    #endregion

    #region 员工公司信息
    public class XYD_UserCompanyInfo
    {
        [Key]
        public int ID { get; set; }
        // 用户ID
        public string EmplID { get; set; }
        // 入职日期
        public DateTime? EmployeeDate { get; set; }
        // 试用日期
        public DateTime? TrialDate { get; set; }
        // 试用期工资
        public float? TrialSalary { get; set; }
        // 劳动合同期限
        public DateTime? ContractDate { get; set; }
        // 转正日期
        public DateTime? FormalDate { get; set; }
        // 转正工资
        public float? FormalSalary { get; set; }
        // 公积金账号
        public string HousingFundNo { get; set; }
        // 社保账号
        public string SocialInsuranceNo { get; set; }
        // 社保缴纳起始月
        public DateTime? SocialInsuranceStartDate { get; set; }
        // 缴纳月数
        public int SocialInsuranceTotalMonth { get; set; }
        // 工资卡号
        public string BankNo { get; set; }
        // 续签次数
        public int ContinueCount { get; set; }
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
        // 型号
        public string Model { get; set; }
        // 单位
        public string Unit { get; set; }
        // 单价
        public decimal? UnitPrice { get; set; }
        // 资产类别
        public string Category { get; set; }
        // 资产状态：可申领，已申领，已报废
        public string Status { get; set; }
        // 地区
        public string Area { get; set; }
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
        // 代码
        public string Code { get; set; }
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
        // 轮播图
        public string Banners { get; set; }
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
        public DateTime? StartTime { get; set; }
        // 结束时间
        public DateTime? EndTime { get; set; }
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
        // 申请ID
        public string MessageID { get; set; }
        // 事假类别
        public string Category { get; set; }
        // 开始时间
        public DateTime StartDate { get; set; }
        // 结束时间
        public DateTime EndDate { get; set; }
        // 事由
        public string Reason { get; set; }
        // 是否批准
        public string Status { get; set; }
        // 创建时间
        public DateTime CreateTime { get; set; }
        // 更新时间
        public DateTime UpdateTime { get; set; }
    }
    #endregion

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

    #region 供应商列表
    public class XYD_Vendor
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 供应商Code
        public string Code { get; set; }
        // 供应商名称 
        public string Name { get; set; }
    }
    #endregion

    #region 凭证列表
    public class XYD_Voucher
    {
        [Key]
        public int ID { get; set; }
        // MessageID
        public string MessageID { get; set; }
        // 制单日期
        public DateTime CreateTime { get; set; }
        // 编号
        public string Sn { get; set; }
        // 科目
        public string VoucherCode { get; set; }
        // 科目名称
        public string VoucherName { get; set; }
        // 金额借方
        public string TotalAmount { get; set; }
        // 额外参数
        public string Extras { get; set; }
        public string User { get; internal set; }
        public string ApplyUser { get; internal set; }
    }
    #endregion

    #region 备用金
    public class XYD_BackupMoney
    {
        // ID
        [Key]
        public int ID { get; set; }
        // 流程ID
        public string MessageID { get; set; }
        // 申请人
        public string EmplID { get; set; }
        // 申请人姓名
        public string EmplName { get; set; }
        // 部门ID
        public string DeptID { get; set; }
        // 所属部门
        public string DeptName { get; set; }
        // 申请类别
        public string Type { get; set; }
        // 申请金额
        public decimal Amount { get; set; }
        // 还款时间
        public DateTime PaybackTime { get; set; }
        // 申请日期
        public DateTime CreateTime { get; set; }
        // 状态
        public string Status { get; set; }
    }
    #endregion

    #region 发票信息表
    public class XYD_InvoiceInfo
    {
        // 发票代码
        [Key, Column("invoiceDataCode", Order = 0)]
        public string invoiceDataCode { get; set; }
        // 发票号
        [Key, Column("invoiceNumber", Order = 1)]
        public string invoiceNumber { get; set; }
        // 科目类别
        public string voucherType { get; set; }
        // 发票类型名称
        public string invoiceTypeName { get; set; }
        // 发票类型
        public string invoiceTypeCode { get; set; }
        // 开票时间
        public string billingTime { get; set; }
        // 校验码 
        public string checkCode { get; set; }
        // 机器码
        public string taxDiskCode { get; set; }
        // 购方名称
        public string purchaserName { get; set; }
        // 购方纳税人识别号
        public string taxpayerNumber { get; set; }
        // 购方银行账号
        public string taxpayerBankAccount { get; set; }
        // 购方地址/电话
        public string taxpayerAddressOrId { get; set; }
        // 销方名称
        public string salesName { get; set; }
        // 销方纳税人识别号
        public string salesTaxpayerNum { get; set; }
        // 销方银行账号
        public string salesTaxpayerBankAccount { get; set; }
        // 销方地址/电话
        public string salesTaxpayerAddress { get; set; }
        // 价税合计
        public string totalTaxSum { get; set; }
        // 税额
        public string totalTaxNum { get; set; }
        // 不含税价（金额）
        public string totalAmount { get; set; }
        // 备注
        public string invoiceRemarks { get; set; }
        // 是否为清单票
        public string isBillMark { get; set; }
        // 作废标志
        public string voidMark { get; set; }
        // 收货员
        public string goodsClerk { get; set; }
        // 收费标志
        public string tollSign { get; set; }
        // 收费标志名称
        public string tollSignName { get; set; }
        // 认证时间
        public DateTime? authenticationTime { get; set; }
        // 作成时间
        public DateTime createdTime { get; set; }
        // 作成者
        public string createdBy { get; set; }
        // 更新时间
        public DateTime updatedTime { get; set; }
        // 更新者
        public string updatedBy { get; set; }
    }
    #endregion

    #region 发票明细表
    public class XYD_InvoiceDetail
    {
        // 发票代码
        [Key, Column("invoiceDataCode", Order = 0)]
        public string invoiceDataCode { get; set; }
        // 发票号
        [Key, Column("invoiceNumber", Order = 1)]
        public string invoiceNumber { get; set; }
        // 行号
        [Key, Column("lineNum", Order = 2)]
        public string lineNum { get; set; }
        // 商品名称
        public string goodserviceName { get; set; }
        // 型号
        public string model { get; set; }
        // 单位
        public string unit { get; set; }
        // 数量
        public string number { get; set; }
        // 价格
        public string price { get; set; }
        // 金额
        public string sum { get; set; }
        // 税率
        public string taxRate { get; set; }
        // 税额
        public string tax { get; set; }
        // 是否为清单行
        public string isBillLine { get; set; }
        // 零税率标志字段
        public string zeroTaxRateSign { get; set; }
        // 零税率标志名称
        public string zeroTaxRateSignName { get; set; }
        // 作成时间
        public DateTime createdTime { get; set; }
        // 作成者
        public string createdBy { get; set; }
        // 更新时间
        public DateTime updatedTime { get; set; }
        // 更新者
        public string updatedBy { get; set; }
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
        public DbSet<XYD_UserInfo> UserInfo { get; set; }
        public DbSet<XYD_Contact> Contact { get; set; }
        public DbSet<XYD_WorkExperience> WorkExperience { get; set; }
        public DbSet<XYD_Education> Education { get; set; }
        public DbSet<XYD_Award> Award { get; set; }
        public DbSet<XYD_Att> Attachment { get; set; }
        public DbSet<XYD_Asset> Asset { get; set; }
        public DbSet<XYD_Asset_Record> AssetRecord { get; set; }
        public DbSet<XYD_Asset_Category> AssetCategory { get; set; }
        public DbSet<XYD_System_Config> SystemConfig { get; set; }
        public DbSet<XYD_MettingBook> MettingBook { get; set; }
        public DbSet<XYD_Leave_Record> LeaveRecord { get; set; }
        public DbSet<XYD_Audit_Record> Audit_Record { get; set; }
        public DbSet<XYD_Vendor> Vendor { get; set; }
        public DbSet<XYD_Voucher> Voucher { get; set; }
        public DbSet<XYD_BackupMoney> BackupMoney { get; set; }
        public DbSet<XYD_InvoiceInfo> InvoiceInfo { get; set; }
        public DbSet<XYD_InvoiceDetail> InvoiceDetail { get; set; }
        public DbSet<XYD_UserCompanyInfo> UserCompanyInfo { get; set; }
    }
    #endregion
}