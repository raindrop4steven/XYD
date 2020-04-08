using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XYD.Models;

namespace XYD.Entity
{
    #region 工资
    public class XYD_Salary
    {
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
}