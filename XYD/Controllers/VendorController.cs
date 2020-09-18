using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Models;
using XYD.Common;
using System.Text;
using XYD.U8Service;
using System.Configuration;
using Newtonsoft.Json;

namespace XYD.Controllers
{
    public class VendorController : Controller
    {
        U8ServiceSoapClient client = new U8ServiceSoapClient();

        #region 供应商列表
        [Authorize]
        public ActionResult List(string Name, int Page = 0, int Size = 10)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var list = db.Vendor.Where(n => true);
                    if (!string.IsNullOrEmpty(Name))
                    {
                        list = list.Where(n => n.Name.Contains(Name));
                    }
                    var totalCount = list.Count();
                    // 记录总页数
                    var totalPage = (int)Math.Ceiling((float)totalCount / Size);
                    var results = list.OrderBy(n => n.ID).Skip(Page * Size).Take(Size).ToList();
                    return ResponseUtil.OK(new
                    {
                        vendors = results,
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

        #region 搜索供应商列表
        [Authorize]
        public ActionResult SearchList(string Name, int Page, int Size)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    Page -= 1;
                    var list = db.Vendor.Where(n => true);
                    if (!string.IsNullOrEmpty(Name))
                    {
                        list = list.Where(n => n.Name.Contains(Name));
                    }
                    var totalCount = list.Count();
                    // 记录总页数
                    var totalPage = (int)Math.Ceiling((float)totalCount / Size);
                    var results = list.OrderBy(n => n.ID).Skip(Page * Size).Take(Size).ToList();
                    return ResponseUtil.OK(new
                    {
                        vendors = results,
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

        #region 供应商详情
        [Authorize]
        public ActionResult Detail(int id)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var vendor = db.Vendor.Where(n => n.ID == id).FirstOrDefault();
                    return ResponseUtil.OK(new
                    {
                        vendor = vendor
                    });
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 增加供应商
        [Authorize]
        public ActionResult Add(XYD_Vendor model)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var newNumber = string.Empty;
                    // 获得最大的番号
                    var maxCode = db.Vendor.Where(n => n.Code.Contains("OAGYS")).OrderByDescending(n => n.Code).FirstOrDefault();
                    if (maxCode == null)
                    {
                        newNumber = 0.ToString("D5");
                    }
                    else
                    {
                        var maxNumber = int.Parse(maxCode.Code.Substring(5, 5));
                        newNumber = (maxNumber + 1).ToString("D5");
                    }
                    var vendor = db.Vendor.Where(n => n.Name == model.Name).FirstOrDefault();
                    if (vendor != null)
                    {
                        return ResponseUtil.Error("不能与现有供应商重复");
                    }
                    // 判断用友里是否已有该供应商
                    var key = ConfigurationManager.AppSettings["WSDL_Key"];
                    var dbresult = client.FindTargetVendor(key, model.Name);
                    var targetVendor = JsonConvert.DeserializeObject<XYD_Vendor>(dbresult);
                    if (targetVendor == null)
                    {
                        model.Code = string.Format("OAGYS{0}", newNumber);
                    }
                    else
                    {
                        model.Code = targetVendor.Code;
                    }
                    db.Vendor.Add(model);
                    db.SaveChanges();
                    return ResponseUtil.OK("添加成功");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 更新供应商
        [Authorize]
        public ActionResult Update(XYD_Vendor model)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var vendor = db.Vendor.Where(n => n.ID == model.ID).FirstOrDefault();
                    if (vendor == null)
                    {
                        return ResponseUtil.Error("供应商不存在");
                    }
                    vendor.Name = model.Name;
                    db.SaveChanges();
                    return ResponseUtil.OK("更新成功");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 删除供应商
        [Authorize]
        public ActionResult Delete(int id)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var vendor = db.Vendor.Where(n => n.ID == id).FirstOrDefault();
                    if (vendor == null)
                    {
                        return ResponseUtil.Error("供应商不存在");
                    }
                    db.Vendor.Remove(vendor);
                    db.SaveChanges();
                    return ResponseUtil.OK("删除成功");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 同步供应商
        /// <summary>
        /// 查找OA中有更新和新增的供应商条目
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult SyncVendor()
        {
            try
            {   
                using (var db = new DefaultConnection())
                {
                    var OACodeList = db.Vendor.Where(n => n.Code.Contains("OAGYS")).ToList();
                    var vendors = JsonConvert.SerializeObject(OACodeList);
                    var key = ConfigurationManager.AppSettings["WSDL_Key"];
                    var result = client.SyncVendor(key, vendors);
                    return ResponseUtil.OK(result);
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