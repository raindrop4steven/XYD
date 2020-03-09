using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Models;
using XYD.Entity;
using System.Text;

namespace XYD.Controllers
{
    public class VoucherController : Controller
    {
        WorkflowMgr mgr = new WorkflowMgr();
        SheetMgr sheetMgr = new SheetMgr();
        OrgMgr orgMgr = new OrgMgr();

        public const string VoucherFormat = "{0},{1},{2},0,{3},{4},{5},{6},{7},{8},{14},,,,,{9},{10},{11},{12},,{13},,,,,,,0,,0,,,,,,,,,0,0,0,0,0,,0,0,0,0";

        #region 记录凭证
        public ActionResult CreateVoucher(string mid, string user, string sn, string total, string extras)
        {
            try
            {
                XYD_SubVoucherCode subCode = null;
                // 申请
                var message = mgr.GetMessage(mid);
                // 获取发起人信息
                var messageIssuedBy = message.MessageIssuedBy;
                var applyUser = orgMgr.GetEmployee(messageIssuedBy);
                // 获取财务
                var auditUser = orgMgr.GetEmployee(user);
                if (string.IsNullOrEmpty(extras))
                {
                    // 科目
                    subCode = WorkflowUtil.GetSubVoucherCode(mid, null);
                }
                

                using (var db = new DefaultConnection())
                {
                    var voucher = new XYD_Voucher();
                    voucher.MessageID = mid;
                    voucher.ApplyUser = message.MessageIssuedBy;
                    voucher.User = user;
                    voucher.CreateTime = DateTime.Now;
                    voucher.VoucherCode = !string.IsNullOrEmpty(extras) ? string.Empty : subCode.Code;
                    voucher.VoucherName = !string.IsNullOrEmpty(extras) ? string.Empty : subCode.Name;
                    voucher.Sn = sn;
                    voucher.TotalAmount = total;
                    voucher.Extras = extras;
                    db.Voucher.Add(voucher);
                    db.SaveChanges();
                }
                return ResponseUtil.OK("记录凭证完成");
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 下载凭证
        /// <summary>
        /// 凭证项目说明		具体请参照文档【新友达服装U8集成方案V3.2-20200214.docx】P29
        /// public const string StrFormat = "{0},{1},{2},0,{3},{4},{5},{6},{7},{8},{14},,,,,{9},{10},{11},{12},,{13},,,,,,,0,,0,,,,,,,,,0,0,0,0,0,,0,0,0,0";
        /// format 名称  说明
        /// {0}	制单日期 以运费发票为例，发票的开票日期
        /// {1}	凭证类别字 默认为【记】
        /// {2}	业务（凭证）号 一张凭证一个号，从1自动采番
        /// {3}	摘要 发票摘要格式：【发票开票时间 发票号 销方名称 费用名称 】例：【2020.01.06 22195129 上海远雅国际物流有限公司 运费】，发票以外的摘要，请咨询张悦
        /// {4}	科目摘要 参照U8提供的文档
        /// {5} 金额借方 借方的人民币金额，如果不是借方，填0
        /// {6}	金额贷方 贷方的人民币金额，如果不是贷方，填0
        /// {7}	数量 数量
        /// {8} 外币金额 如果有外币的情况下填，没有外币填0
        /// {9}	部门编码 不需要部门编码时，空
        /// {10}	个人编码 不需要个人编码时，空
        /// {11}	客户编码 不需要客户编码时，空
        /// {12}	供应商编码 不需要供应商编码时，空
        /// {13}	款号编码 不需要款号编码时，空
        /// {14}	汇率 没有外币时填0
        /// </summary>
        /// <param name="BeginDate"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        public ActionResult DownloadVoucher(DateTime BeginDate, DateTime EndDate)
        {
            try
            {
                /*
                 * 变量定义
                 */
                var Results = new List<string>();

                // 获取开始月份第一天，结束月份最后一天
                BeginDate = new DateTime(BeginDate.Year, BeginDate.Month, 1);
                EndDate = EndDate.AddMonths(1).AddDays(-1);
                // 记录列表
                EndDate = CommonUtils.EndOfDay(EndDate);
                using (var db = new DefaultConnection())
                {
                    var list = db.Voucher.Where(n => n.CreateTime > BeginDate.Date && n.CreateTime <= EndDate).OrderBy(n => n.CreateTime).ToList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        var record = list.ElementAt(i);
                        // 制单时间
                        var CreateTime = record.CreateTime.ToString("yyyy-MM-dd");
                        // 业务编号
                        var index = i + 1;
                        // 科目摘要
                        var VoucherCode = record.VoucherCode;
                        // 科目名称
                        var VoucherName = record.VoucherName;
                        // 编号
                        var Sn = record.Sn;
                        // 发起人
                        var ApplyUser = orgMgr.GetEmployee(record.ApplyUser);
                        // 制单人
                        var User = orgMgr.GetEmployee(record.User);
                        // 摘要
                        var brief = string.Format("{0} {1} 付 {2} {3}", CreateTime, Sn, VoucherName, User.EmplName);

                        string voucher = string.Empty;
                        if (string.IsNullOrEmpty(record.Extras))
                        {
                            // 科目已确定，一条借
                            voucher = string.Format(VoucherFormat, CreateTime, "记", index, brief, VoucherCode, record.TotalAmount, 0, string.Empty, 0, string.Empty, ApplyUser.EmplNO, string.Empty, string.Empty, string.Empty, string.Empty);
                            Results.Add(voucher);
                            // 一条贷
                            voucher = string.Format(VoucherFormat, CreateTime, "记", index, brief, VoucherCode, 0, record.TotalAmount, string.Empty, 0, string.Empty, ApplyUser.EmplNO, string.Empty, string.Empty, string.Empty, string.Empty);
                            Results.Add(voucher);
                        }
                        else
                        {
                            // 科目未确定，多条借一条贷
                            Dictionary<string, double> voucherDict = new Dictionary<string, double>();
                            var lineArray = record.Extras.Split(';').ToList();
                            foreach (var lineData in lineArray)
                            {
                                var subLineArray = lineData.Split(',').ToList();
                                var subLineName = subLineArray.ElementAt(0);
                                if (string.IsNullOrEmpty(subLineName))
                                {
                                    continue;
                                }
                                var subAmountArray = subLineArray.Skip(1).Take(subLineArray.Count - 1);
                                var subTotal = subAmountArray.Where(n => !string.IsNullOrEmpty(n)).Sum(n => double.Parse(n));
                                // 获得科目
                                // 科目
                                XYD_SubVoucherCode subCode = WorkflowUtil.GetSubVoucherCode(record.MessageID, subLineName);
                                if (!voucherDict.Keys.Contains(subCode.Code))
                                {
                                    voucherDict[subCode.Code] = subTotal;
                                }
                                else
                                {
                                    voucherDict[subCode.Code] += subTotal; 
                                }
                            }
                            // 多条借
                            foreach (var item in voucherDict)
                            {
                                // 科目已确定，一条借
                                voucher = string.Format(VoucherFormat, CreateTime, "记", index, brief, item.Key, item.Value, 0, string.Empty, 0, string.Empty, ApplyUser.EmplNO, string.Empty, string.Empty, string.Empty, string.Empty);
                                Results.Add(voucher);
                            }
                            // 一条贷
                            voucher = string.Format(VoucherFormat, CreateTime, "记", index, brief, VoucherCode, 0, record.TotalAmount, string.Empty, 0, string.Empty, ApplyUser.EmplNO, string.Empty, string.Empty, string.Empty, string.Empty);
                            Results.Add(voucher);
                        }
                    }
                    // 数据转换成文件下载
                    var sb = new StringBuilder();
                    sb.AppendLine("填制凭证,V800");
                    foreach (var data in Results)
                    {
                        sb.AppendLine(data);
                    }
                    return File(new UTF8Encoding().GetBytes(sb.ToString()), "text/plain", "Voucher.txt");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 列表
        public ActionResult List(DateTime BeginDate, DateTime EndDate, int Page, int Size)
        {
            try
            {
                // 获取开始月份第一天，结束月份最后一天
                BeginDate = new DateTime(BeginDate.Year, BeginDate.Month, 1);
                EndDate = EndDate.AddMonths(1).AddDays(-1);
                // 记录列表
                EndDate = CommonUtils.EndOfDay(EndDate);
                using (var db = new DefaultConnection())
                {
                    var list = db.Voucher.Where(n => n.CreateTime >= BeginDate.Date && n.CreateTime <= EndDate).OrderByDescending(n => n.CreateTime ).ToList();
                    var totalCount = list.Count();
                    var results = list.Skip(Page * Size).Take(Size);
                    var totalPage = (int)Math.Ceiling((float)totalCount / Size);

                    var resultVouchers = new List<object>();
                    foreach(var voucher in results)
                    {
                        var applyUser = orgMgr.GetEmployee(voucher.ApplyUser).EmplName;
                        var auditUser = orgMgr.GetEmployee(voucher.User).EmplName;
                        var message = mgr.GetMessage(voucher.MessageID);
                        resultVouchers.Add(new
                        {
                            ID = voucher.ID,
                            Message = message.MessageTitle,
                            Sn = voucher.Sn,
                            CreateTime = voucher.CreateTime,
                            TotalAmount = voucher.TotalAmount,
                            ApplyUser = applyUser,
                            AuditUser = auditUser
                        });
                    }
                    return ResponseUtil.OK(new
                    {
                        results = resultVouchers,
                        meta = new
                        {
                            current_page = Page,
                            total_page = totalPage,
                            current_count = Page * Size + results.Count(),
                            total_count = totalCount,
                            per_page = Size
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