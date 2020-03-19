using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Models;
using XYD.Common;
using XYD.Entity;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Appkiz.Library.Security.Authentication;

namespace XYD.Controllers
{
    public class InvoiceController : Controller
    {
        #region 列表
        public ActionResult List(string salesName, string invoiceNumber, DateTime? authenticationTime, int Page = 0, int Size = 10)
        {
            try
            {
                var db = new DefaultConnection();
                var query = db.InvoiceInfo.Where(n => true);
                if (!string.IsNullOrEmpty(salesName))
                {
                    query = query.Where(n => n.salesName.Contains(salesName));
                }
                if (!string.IsNullOrEmpty(invoiceNumber))
                {
                    query = query.Where(n => n.invoiceNumber.Contains(invoiceNumber));
                }
                if (authenticationTime != null)
                {
                    var firstDayOfMonth = new DateTime(authenticationTime.Value.Year, authenticationTime.Value.Month, 1);
                    var lastDayOfMonth = CommonUtils.EndOfDay(firstDayOfMonth.AddMonths(1).AddDays(-1));
                    query = query.Where(n => n.authenticationTime.Value >= firstDayOfMonth.Date && n.authenticationTime.Value <= lastDayOfMonth);
                }
                var dataQuery = query;
                int totalCount = query.Count();
                var list = dataQuery.OrderByDescending(n => n.createdTime).Skip(Page * Size).Take(Size);
                var totalPage = (int)Math.Ceiling((float)totalCount / Size);
                // 发票详情
                var resultList = new List<XYD_Invoice>() ;
                foreach(XYD_InvoiceInfo invoiceInfo in list)
                {
                    XYD_Invoice invoice = (XYD_Invoice)invoiceInfo;
                    invoice.invoiceDetailData = db.InvoiceDetail.Where(n => n.invoiceDataCode == invoiceInfo.invoiceDataCode && n.invoiceNumber == invoiceInfo.invoiceNumber).ToList();
                    resultList.Add(invoice);
                }
                return ResponseUtil.OK(new
                {
                    records = resultList,
                    meta = new
                    {
                        current_page = Page,
                        total_page = totalPage,
                        current_count = Page * Size + list.Count(),
                        total_count = totalCount,
                        per_page = Size
                    }
                });
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 新增
        [Authorize]
        public ActionResult Add()
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                Stream stream = Request.InputStream;
                stream.Seek(0, SeekOrigin.Begin);
                string json = new StreamReader(stream).ReadToEnd();

                // 解析成发票信息
                XYD_Invoice invoice = JsonConvert.DeserializeObject<XYD_Invoice>(json);
                using (var db = new DefaultConnection())
                {
                    XYD_InvoiceInfo invoiceInfo = invoice;
                    invoiceInfo.createdBy = employee.EmplID;
                    invoiceInfo.updatedBy = employee.EmplID;
                    invoiceInfo.createdTime = DateTime.Now;
                    invoiceInfo.updatedTime = DateTime.Now;
                    List<XYD_InvoiceDetail> invoiceDetails = new List<XYD_InvoiceDetail>();
                    foreach (XYD_InvoiceDetail detail in invoice.invoiceDetailData)
                    {
                        detail.invoiceDataCode = invoice.invoiceDataCode;
                        detail.invoiceNumber = invoice.invoiceNumber;
                        detail.createdBy = employee.EmplID;
                        detail.updatedBy = employee.EmplID;
                        detail.createdTime = DateTime.Now;
                        detail.updatedTime = DateTime.Now;
                        invoiceDetails.Add(detail);
                    }
                    db.InvoiceInfo.Add(invoice);
                    db.InvoiceDetail.AddRange(invoiceDetails);
                    db.SaveChanges();
                    return ResponseUtil.OK("添加成功");
                }
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 根据发票信息获得发票
        public ActionResult GetInvoiceInfo(string invoiceCode, string invoiceNumber, string billTime, string invoiceAmount, string checkCode)
        {
            try
            {
                var msg = string.Empty;
                var invoiceResult = string.Empty;
                var free = false;
                var flag = InvoiceHelper.GetInvoiceInfo(invoiceCode, invoiceNumber, billTime, invoiceAmount, checkCode, ref msg, ref invoiceResult, ref free);
                if (flag)
                {
                    var invoiceData = (JObject)JsonConvert.DeserializeObject(invoiceResult);
                    return ResponseUtil.OK(invoiceData);
                } else
                {
                    return ResponseUtil.Error(msg);
                }
                
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 根据QRCode获得发票信息
        public ActionResult GetInvoiceByQrCode(string scanStr)
        {
            try
            {
                var msg = string.Empty;
                var invoiceResult = string.Empty;
                var free = false;
                var flag = InvoiceHelper.GetInvoiceInfoByQRCode(scanStr, ref msg, ref invoiceResult, ref free);
                if (flag)
                {
                    var invoiceData = (JObject)JsonConvert.DeserializeObject(invoiceResult);
                    return ResponseUtil.OK(invoiceData);
                }
                else
                {
                    return ResponseUtil.Error(msg);
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 删除
        public ActionResult Delete()
        {
            return null;
        }
        #endregion

        #region 科目列表
        public ActionResult VoucherTypes()
        {
            try
            {
                var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", "voucherOptions"));
                using (StreamReader sr = new StreamReader(filePathName))
                {
                    var Credits = JsonConvert.DeserializeObject<XYD_VoucherOptions>(sr.ReadToEnd());
                    return ResponseUtil.OK(Credits);
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 认证
        [Authorize]
        public ActionResult Auth()
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var employee = (User.Identity as AppkizIdentity).Employee;
                    Stream stream = Request.InputStream;
                    stream.Seek(0, SeekOrigin.Begin);
                    string json = new StreamReader(stream).ReadToEnd();
                    var invoiceAuth = JsonConvert.DeserializeObject<XYD_Invoice_Auth>(json);
                    foreach(XYD_Invoice_Key key in invoiceAuth.keys)
                    {
                        var invoiceInfo = db.InvoiceInfo.Where(n => n.invoiceDataCode == key.invoiceDataCode && n.invoiceNumber == key.invoiceNumber).FirstOrDefault();
                        if (invoiceInfo != null)
                        {
                            invoiceInfo.authenticationTime = invoiceAuth.authenticationTime;
                            invoiceInfo.updatedBy = employee.EmplID;
                            invoiceInfo.updatedTime = DateTime.Now;
                        }
                    }
                    db.SaveChanges();
                    return ResponseUtil.OK("认证成功");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion
    }
}