using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XYD.Models;
using static XYD.Common.DEP_Constants;

namespace XYD.Entity
{
    #region 工资
    public class XYD_Salary
    {
        public decimal ShouldPay { get; set; }
        public decimal Salary { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
    #endregion

    #region 流程数量
    public class XYD_DB_Message_Count
    {
        public string WorkflowId { get; set; }
        public int MessageCount { get; set; }
        public string MessageTitle { get; set; }
        public string FolderName { get; set; }
    }
    #endregion

    #region U8 用户
    public class XYD_U8_Person
    {
        public string cPersonCode { get; set; }
        public string cPersonName { get; set; }
        public string cDepCode { get; set; }
    }
    #endregion

    #region 发票信息
    public class XYD_Invoice : XYD_InvoiceInfo
    {
        public List<XYD_InvoiceDetail> invoiceDetailData { get; set; }
    }
    #endregion

    #region 认证发票
    public class XYD_Invoice_Auth
    {
        public DateTime authenticationTime { get; set; }
        public List<XYD_Invoice_Key> keys { get; set; }
    }

    public class XYD_Invoice_Key
    {
        public string invoiceDataCode { get; set; }
        public string invoiceNumber { get; set; }
    }
    #endregion

    #region 发布文章通知
    public class XYD_CMS_Notification
    {
        public string url { get; set; }
        public string title { get; set; }
        public List<string> unsavedReaders { get; set; }
    }
    #endregion

    #region 备用金
    public class XYD_BackupMoneyReport
    {
        public string EmplName;
        public string DeptName;
        public decimal Amount;
    }
    #endregion

    #region 考勤数据
    public class XYD_Calendar_Result
    {
        public List<XYD_CalendarEntity> dates;
        public List<XYD_CalendarDetail> details;
        public Dictionary<CALENDAR_TYPE, int> summary;
    }

    public class XYD_Calendar_Report
    {
        public string EmplID;
        public string EmplName;
        public string EmplNo;
        public string DeptName;
        public string Position;
        public int ShouldWorkDays;
        public Dictionary<CALENDAR_TYPE, int> summary;
    }
    #endregion

    #region 出勤记录
    public class XYD_Leave_Result
    {
        // ID
        public int ID { get; set; }
        // 请假人
        public string EmplID { get; set; }
        // 姓名
        public string EmplName { get; set; }
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
    }
    #endregion

    #region 出勤记录
    public class XYD_Leave_Search
    {
        public string EmplID { get; set; }
        // 姓名
        public string EmplName { get; set; }
        // 工号
        public string EmplNO { get; set; }
        // 事假类别
        public string Category { get; set; }
        // 开始时间
        public DateTime StartDate { get; set; }
        // 结束时间
        public DateTime? EndDate { get; set; }
        // 事由
        public string Reason { get; set; }
        // 时间类型
        public string TimeType { get; set; }
        // 小时数
        public double Hours { get; set; }
    }
    #endregion

    #region 用户统计信息
    public class XYD_UserReport
    {
        // 用户ID
        public string EmplID;
        // 姓名
        public string EmplName;
        // 性别	
        public string EmplSex;
        // 编号	
        public string EmplNO;
        // 部门	
        public string DeptName;
        // 职位	
        public string PositionName;
        // 学历	
        public string Education;
        // 出生年月	
        public string EmplBirth;
        // 身份证号码	
        public string CredNo;
        // 手机	
        public string Mobile;
        // 电子邮件	
        public string Email;
        // 招行卡号	
        public string BankNo;
        // 入职日期	
        public string EmployeeDate;
        // 转正日期	
        public string FormalDate;
        // 合同到期日	
        public string ContractDate;
        // 累计缴费月数	
        public int SocialInsuranceTotalMonth;
        // 户籍地址	
        public string Residence;
        // 居住地	
        public string CurrentAddress;
        // 紧急联系人	
        public string EmergencyContact;
        // 联系人手机
        public string EmergencyMobile;

    }
    #endregion

    #region 假期统计
    public class XYD_Vocation_Report
    {
        // 年假
        public double yearHour;
        // 加班
        public double extraHour;
        // 事假、调休
        public double leaveHour;
        // 病假
        public double sickHour;
        // 婚假
        public double marryHour;
        // 产假
        public double birthHour;
        // 哺乳假
        public double milkHour;
        // 丧假
        public double deadHour;
        // 特殊调整
        public double adjustHour;
    }
    #endregion
}