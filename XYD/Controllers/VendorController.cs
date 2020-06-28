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
                    var maxCode = db.Vendor.OrderByDescending(n => n.Code).FirstOrDefault();
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
                    model.Code = string.Format("OAGYS{0}", newNumber);
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
                var sql = @"SELECT
	                                    cVenCode AS Code,
	                                    cVenName AS Name 
                                    FROM
	                                    Vendor 
                                    WHERE
	                                    cVenCode LIKE 'OA%'";
                var allSql = @"SELECT
	                                    cVenCode AS Code,
	                                    cVenName AS Name 
                                    FROM
	                                    Vendor";
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["YongYouConnection"].ConnectionString;
                var result = DbUtil.ExecuteSqlCommand(connectionString, sql, DbUtil.GetUnSyncVendor);
                var allResult = DbUtil.ExecuteSqlCommand(connectionString, allSql, DbUtil.GetUnSyncVendor);
                StringBuilder sb = new StringBuilder();
                using (var db = new DefaultConnection())
                {
                    var OACodeList = db.Vendor.ToList();
                    var InsertList = new List<string>();
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
                            if (!string.IsNullOrEmpty(oaVendor.Name))
                            {
                                sb.Append(string.Format("UPDATE Vendor SET cVenName = '{0}', cVenAbbName = '{1}' WHERE cVenCode = '{2}';", oaVendor.Name, oaVendor.Name, oaVendor.Code));
                                InsertList.Add(oaVendor.Name);
                            }
                        }
                        else if (flag == 3)
                        {
                            if (!string.IsNullOrEmpty(oaVendor.Name))
                            {
                                sb.Append(string.Format("INSERT INTO Vendor ( cVenCode, cVenName, cVenAbbName) VALUES( '{0}', '{1}', '{2}');", oaVendor.Code, oaVendor.Name, oaVendor.Name));
                                InsertList.Add(oaVendor.Name);
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    // 检查是否有名称相同但Code不同的
                    if (InsertList.Count > 0)
                    {
                        var ConflictVendors = new List<string>();
                        foreach(XYD_Vendor allU8Vendor in allResult)
                        {
                            if (InsertList.Contains(allU8Vendor.Name))
                            {
                                ConflictVendors.Add(allU8Vendor.Name);
                            }
                        }
                        if (ConflictVendors.Count > 0)
                        {
                            var ConflictMsg = string.Join(",", ConflictVendors);
                            return ResponseUtil.Error(string.Format("供应商已存在:{0}",ConflictMsg));
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