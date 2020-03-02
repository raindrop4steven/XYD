using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Models;
using XYD.Common;

namespace XYD.Controllers
{
    public class VendorController : Controller
    {
        #region 供应商列表
        [Authorize]
        public ActionResult List(int Page = 0, int Size = 10)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var list = db.Vendor.Where(n => true);
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
                    var vendor = db.Vendor.Where(n => n.Code == model.Code).FirstOrDefault();
                    if (vendor != null)
                    {
                        return ResponseUtil.Error("不能与现有供应商代码重复");
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
                    vendor.Code = model.Code;
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
        [Authorize]
        public ActionResult SyncVendor()
        {
            try
            {
                List<string> CodeList;
                using (var db = new DefaultConnection())
                {
                    CodeList = db.Vendor.Select(n => "'" + n.Code + "'").ToList();
                }
                
                var sql = @"SELECT
	                                    cVenCode AS Code,
	                                    cVenName AS Name 
                                    FROM
	                                    Vendor 
                                    WHERE
	                                    cVenCode LIKE 'OA%'";
                
                if (CodeList.Count > 0)
                {
                    var InString = string.Join(",", CodeList);
                    sql = string.Format(@" {0} AND cVenCode NOT IN ({1})", sql, InString);
                }
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["YongYouConnection"].ConnectionString;
                var result = DbUtil.ExecuteSqlCommand(connectionString, sql, DbUtil.GetUnSyncVendor);
                using (var db = new DefaultConnection())
                {
                    foreach (XYD_Vendor vendor in result)
                    {
                        db.Vendor.Add(vendor);
                        db.SaveChanges();
                    }
                }
                return ResponseUtil.OK(result.Count() == 0 ? "供应商记录已经是最新的" : string.Format("新同步{0}条供应商记录", result.Count()));
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion
    }
}