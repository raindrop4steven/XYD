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
using Appkiz.Library.Security;
using ClosedXML.Excel;

namespace XYD.Controllers
{
    [LogException]
    public class InvoiceController : Controller
    {

        OrgMgr orgMgr = new OrgMgr();

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
                var voucherOptions = WorkflowUtil.GetVoucherOptions().Options.ToDictionary(x => x.Code, x=> x.Name);
                var resultList = new List<XYD_Invoice>() ;
                foreach(XYD_InvoiceInfo invoiceInfo in list)
                {
                    XYD_Invoice invoice = (XYD_Invoice)invoiceInfo;
                    invoice.createdBy = orgMgr.GetEmployee(invoice.createdBy).EmplName;
                    invoice.voucherType = voucherOptions[invoice.voucherType];
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
                checkExistInvoice(invoice.invoiceDataCode, invoice.invoiceNumber);
                using (var db = new DefaultConnection())
                {
                    XYD_InvoiceInfo invoiceInfo = invoice;
                    invoiceInfo.createdBy = employee.EmplID;
                    invoiceInfo.updatedBy = employee.EmplID;
                    invoiceInfo.createdTime = DateTime.Now;
                    invoiceInfo.updatedTime = DateTime.Now;
                    invoiceInfo.authenticationTime = null;
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

        #region 判断重复发票
        public void checkExistInvoice(string invoiceCode, string invoiceNumber)
        {
            using (var db = new DefaultConnection())
            {
                var invoiceInfo = db.InvoiceInfo.Where(n => n.invoiceDataCode == invoiceCode && n.invoiceNumber == invoiceNumber).FirstOrDefault();
                if (invoiceInfo != null)
                {
                    throw new Exception("数据库已存在相同发票");
                }
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

        public Dictionary<string, string> GetVoucherOptions()
        {
            var filePathName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"], string.Format("{0}.json", "voucherOptions"));
            using (StreamReader sr = new StreamReader(filePathName))
            {
                var Credits = JsonConvert.DeserializeObject<XYD_VoucherOptions>(sr.ReadToEnd());
                var optionDict = Credits.Options.ToDictionary(x => x.Code, x => x.Name);
                return optionDict;
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
                    var vouchers = new List<XYD_Voucher>();
                    var optionDict = GetVoucherOptions();
                    foreach(XYD_Invoice_Key key in invoiceAuth.keys)
                    {
                        var invoiceInfo = db.InvoiceInfo.Where(n => n.invoiceDataCode == key.invoiceDataCode && n.invoiceNumber == key.invoiceNumber).FirstOrDefault();
                        if (invoiceInfo != null)
                        {
                            if (invoiceInfo.authenticationTime != null)
                            {
                                return ResponseUtil.Error("不能重复认证");
                            }
                            // 发票
                            invoiceInfo.authenticationTime = invoiceAuth.authenticationTime;
                            invoiceInfo.updatedBy = employee.EmplID;
                            invoiceInfo.updatedTime = DateTime.Now;
                            // 凭证
                            var vendor = db.Vendor.Where(n => n.Name == invoiceInfo.salesName).FirstOrDefault();
                            if (vendor == null)
                            {
                                return ResponseUtil.Error("销方不在供应商列表中");
                            }
                            vouchers.Add(new XYD_Voucher
                            {
                                MessageID = DEP_Constants.INVOICE_WORKFLOW_ID,
                                InvoiceDataCode = invoiceInfo.invoiceDataCode,
                                InvoiceNumber = invoiceInfo.invoiceNumber,
                                CreateTime = invoiceAuth.authenticationTime,
                                VoucherCode = invoiceInfo.voucherType,
                                VoucherName = optionDict[invoiceInfo.voucherType],
                                TotalAmount = invoiceInfo.totalTaxSum,
                                TotalTaxNum = invoiceInfo.totalTaxNum,
                                TotalTaxFreeNum = invoiceInfo.totalAmount,
                                User = invoiceInfo.createdBy,
                                ApplyUser = invoiceInfo.updatedBy,
                                VendorNo = vendor.Code,
                                DeptNo = invoiceInfo.deptNo,
                                Extras = invoiceInfo.voucherType == DEP_Constants.INVOICE_VOUCHER_TYPE_EXPRESS ? invoiceInfo.express : string.Empty
                            });
                        }
                    }
                    db.Voucher.AddRange(vouchers);
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

        #region 部门列表
        [Authorize]
        public ActionResult DeptList()
        {
            var depts = new List<object>()
            {
                new
                {
                    Code = DEP_Constants.INVOICE_DEPT_SH_CODE,
                    Name = DEP_Constants.INVOICE_DEPT_SH_NAME
                },
                new
                {
                    Code = DEP_Constants.INVOICE_DEPT_WX_CODE,
                    Name = DEP_Constants.INVOICE_DEPT_WX_NAME
                }
            };
            return ResponseUtil.OK(depts);
        }
        #endregion

        #region 上传快递费用Excel
        [Authorize]
        public ActionResult UploadExpress()
        {
            try
            {
                // 个人上传文件夹检查
                var folder = System.Configuration.ConfigurationManager.AppSettings["UploadPath"];
                var folderPath = folder;
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // 提取上传的文件
                HttpPostedFileBase file = Request.Files[0];

                // 原文件名 Test.txt
                var fileName = Path.GetFileName(file.FileName);
                // 最终文件名 yyyyMMddHHmmss+4random.txt
                var finalFileName = DiskUtil.GetFinalFileName(fileName);
                // 最终存储路径
                var path = Path.Combine(folderPath, finalFileName);

                // 保存文件
                file.SaveAs(path);

                // 读取Excel中内容
                Dictionary<string, decimal> expressDict = new Dictionary<string, decimal>();
                using (XLWorkbook wb = new XLWorkbook(path))
                {
                    var ws = wb.Worksheets.First();
                    var rows = ws.RowsUsed().Skip(1);
                    foreach(var row in rows)
                    {
                        string senderName = row.Cell(1).GetString();
                        var amount = decimal.Parse(row.Cell(6).GetString());
                        if (expressDict.ContainsKey(senderName))
                        {
                            expressDict[senderName] += amount;
                        }
                        else
                        {
                            expressDict[senderName] = amount;
                        }
                    }
                }
                // 要存储的内容
                List<XYD_Express> expressList = new List<XYD_Express>();
                foreach(var senderName in expressDict.Keys)
                {
                    var employee = orgMgr.FindEmployee("EmplName=@EmplName", new System.Collections.Hashtable()
                      {
                        {
                          "@EmplName",
                          senderName
                        }
                      }, string.Empty, 0, 1).FirstOrDefault();
                    expressList.Add(new XYD_Express()
                    {
                        SenderId = employee.EmplID,
                        Amount = expressDict[senderName],
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now
                    });
                }

                using (var db = new DefaultConnection())
                {
                    // 存储快递费用
                    db.Express.AddRange(expressList);
                    // 存储上传文件
                    var att = new XYD_Att();
                    att.Name = fileName;
                    att.Path = Path.Combine(finalFileName).Replace("\\", "/");
                    db.Attachment.Add(att);
                    db.SaveChanges();
                    return new JsonNetResult(new
                    {
                        status = 200,
                        data = new
                        {
                            id = att.ID,
                            name = att.Name,
                            path = Url.Action("Download", "Common", new { id = att.ID }),
                            express = expressList.Select(n => n.ID)
                        }
                    });
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