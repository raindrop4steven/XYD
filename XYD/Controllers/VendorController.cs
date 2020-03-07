using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Models;
using XYD.Common;
using System.Text;

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
        /// <summary>
        /// 查找OA中有更新和新增的供应商条目
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult SyncVendor()
        {
            try
            {   
                var sql = @"SELECT
	                                    cVenCode AS Code,
	                                    cVenName AS Name 
                                    FROM
	                                    Vendor 
                                    WHERE
	                                    cVenCode LIKE 'OA%'";
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["YongYouConnection"].ConnectionString;
                var result = DbUtil.ExecuteSqlCommand(connectionString, sql, DbUtil.GetUnSyncVendor);
                StringBuilder sb = new StringBuilder();
                using (var db = new DefaultConnection())
                {
                    var OACodeList = db.Vendor.ToList();
                    foreach(XYD_Vendor oaVendor in OACodeList)
                    {
                        int flag = 0;
                        foreach(XYD_Vendor u8Vendor in result)
                        {
                            if (u8Vendor.Code == oaVendor.Code)
                            {
                                if (u8Vendor.Name == oaVendor.Name)
                                {
                                    flag = 1;
                                    break;
                                }
                                else
                                {
                                    flag = 2;
                                    break;
                                }
                            }
                            else
                            {
                                flag = 3;
                            }
                        }
                        // 检查flag
                        if (flag == 2)
                        {
                            sb.Append(string.Format("UPDATE Vendor SET cVenName = '{0}', cVenAbbName = '{1}' WHERE cVenCode = '{2}';", oaVendor.Name, oaVendor.Name, oaVendor.Code));
                        }
                        else if (flag == 3)
                        {
                            sb.Append(string.Format("INSERT INTO Vendor ( cVenCode, cVenName, cVenAbbName) VALUES( '{0}', '{1}', '{2}');", oaVendor.Code, oaVendor.Name, oaVendor.Name));
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                // 检查是否有更新
                var batchSql = sb.ToString();
                if (!string.IsNullOrEmpty(batchSql))
                {
                    DbUtil.ExecuteSqlCommand(connectionString, batchSql);
                    return ResponseUtil.OK("同步成功");
                }
                else
                {
                    return ResponseUtil.OK("记录已是最新，无需同步");
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