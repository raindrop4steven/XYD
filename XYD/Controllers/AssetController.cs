﻿using Appkiz.Library.Security.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Models;

namespace XYD.Controllers
{
    public class AssetController : Controller
    {
        #region 资产导入
        /// <summary>
        /// assets规则：资产名称，型号，数量，单位，单价;资产名称，型号，数量，单位，单价
        /// </summary>
        /// <param name="assets"></param>
        /// <returns></returns>
        public ActionResult Import(string area, string assets)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var lines = assets.Split(';').ToList();
                    List<XYD_Asset> list = new List<XYD_Asset>();
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrEmpty(line.Replace(",", "")))
                        {
                            continue;
                        }
                        else
                        {
                            var cols = line.Split(',').ToList();
                            if (cols.Count < 2)
                            {
                                continue;
                            }
                            else
                            {
                                string name = cols.ElementAt(0);
                                string model = cols.ElementAt(1);
                                int count = int.Parse(cols.ElementAt(2));
                                string unit = cols.ElementAt(3);
                                decimal unitPrice = decimal.Parse(cols.ElementAt(4));
                                string memo = cols.ElementAt(2);
                                string category = unitPrice >= 3000 ? DEP_Constants.ASSET_CATEGORY_ASSET : DEP_Constants.ASSET_CATEGORY_CONSUME;
                                for (int i = 0; i < count; i++)
                                {
                                    XYD_Asset asset = new XYD_Asset();
                                    asset.Sn = DiskUtil.GetFormNumber();
                                    asset.Name = name;
                                    asset.Model = model;
                                    asset.Unit = unit;
                                    asset.UnitPrice = unitPrice;
                                    asset.Category = category;
                                    asset.Status = DEP_Constants.Asset_Status_Available;
                                    asset.Area = area;
                                    asset.CreateTime = DateTime.Now;
                                    asset.UpdateTime = DateTime.Now;
                                    list.Add(asset);
                                }
                            }
                        }
                    }
                    if (list.Count > 0)
                    {
                        db.Asset.AddRange(list);
                        db.SaveChanges();
                    }
                    return ResponseUtil.OK("添加成功");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 资产添加（未使用）
        [Authorize]
        public ActionResult Add(XYD_Asset model)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                using (var db = new DefaultConnection())
                {
                    // 添加资产
                    model.Status = DEP_Constants.Asset_Status_Available;
                    model.CreateTime = DateTime.Now;
                    model.UpdateTime = DateTime.Now;
                    db.Asset.Add(model);
                    db.SaveChanges();
                    // 添加记录
                    var record = new XYD_Asset_Record();
                    record.AssetID = model.ID;
                    record.Operation = DEP_Constants.Asset_Operation_Add;
                    record.EmplName = employee.EmplName;
                    record.DeptName = employee.DeptName;
                    record.CreateTime = DateTime.Now;
                    record.UpdateTime = DateTime.Now;
                    db.AssetRecord.Add(record);
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

        #region 资产类别列表
        [Authorize]
        public ActionResult CategoryList()
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                using (var db = new DefaultConnection())
                {
                    var list = db.AssetCategory.OrderBy(n => n.Order).Select(n => new {
                        Code = n.Code,
                        Name = n.Name
                    }).ToList();
                    return ResponseUtil.OK(list);
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 资产状态列表
        [Authorize]
        public ActionResult StatusList()
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                var statusList = new List<object>
                {
                    new
                    {
                        key = DEP_Constants.Asset_Status_Available,
                        value = "可申领"
                    },
                    new
                    {
                        key = DEP_Constants.Asset_Status_Used,
                        value = "已申领"
                    },
                    new
                    {
                        key = DEP_Constants.Asset_Status_Scraped,
                        value = "已报废"
                    }
                };
                return ResponseUtil.OK(statusList);
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 编辑资产信息
        [Authorize]
        public ActionResult Update(XYD_Asset model)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                using (var db = new DefaultConnection())
                {
                    var entity = db.Asset.Where(n => n.ID == model.ID).FirstOrDefault();
                    if (entity == null)
                    {
                        return ResponseUtil.Error("记录不存在");
                    }
                    CommonUtils.CopyProperties<XYD_Asset>(model, entity);
                    entity.UpdateTime = DateTime.Now;
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

        #region 资产详情
        [Authorize]
        public ActionResult Detail(int id)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                using (var db = new DefaultConnection())
                {
                    var entity = db.Asset.Where(n => n.ID == id).FirstOrDefault();
                    return ResponseUtil.OK(new {
                        emplName = employee.EmplName,
                        detail = entity
                    });
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 资产申领
        [Authorize]
        public ActionResult Apply(int id)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                using (var db = new DefaultConnection())
                {
                    var entity = db.Asset.Where(n => n.ID == id).FirstOrDefault();
                    if (entity == null)
                    {
                        return ResponseUtil.Error("记录不存在");
                    }
                    if (entity.Status != DEP_Constants.Asset_Status_Available)
                    {
                        return ResponseUtil.Error("当前资产不可申领");
                    }
                    entity.Status = DEP_Constants.Asset_Status_Used;
                    entity.UpdateTime = DateTime.Now;
                    // 记录
                    var record = new XYD_Asset_Record();
                    record.AssetID = entity.ID;
                    record.Operation = DEP_Constants.Asset_Operation_Apply;
                    record.EmplName = employee.EmplName;
                    record.DeptName = employee.DeptName;
                    record.CreateTime = DateTime.Now;
                    record.UpdateTime = DateTime.Now;
                    db.AssetRecord.Add(record);
                    db.SaveChanges();
                    return ResponseUtil.OK("申领成功");
                }
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 资产归还
        [Authorize]
        public ActionResult Return(int id)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                using (var db = new DefaultConnection())
                {
                    var entity = db.Asset.Where(n => n.ID == id).FirstOrDefault();
                    if (entity == null)
                    {
                        return ResponseUtil.Error("记录不存在");
                    }
                    if (entity.Status != DEP_Constants.Asset_Status_Used)
                    {
                        return ResponseUtil.Error("当前资产不可归还");
                    }
                    entity.Status = DEP_Constants.Asset_Status_Available;
                    entity.UpdateTime = DateTime.Now;
                    // 记录
                    var record = new XYD_Asset_Record();
                    record.AssetID = entity.ID;
                    record.Operation = DEP_Constants.Asset_Operation_Return ;
                    record.EmplName = employee.EmplName;
                    record.DeptName = employee.DeptName;
                    record.CreateTime = DateTime.Now;
                    record.UpdateTime = DateTime.Now;
                    db.AssetRecord.Add(record);
                    db.SaveChanges();
                    return ResponseUtil.OK("归还成功");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 资产报销
        [Authorize]
        public ActionResult Scrap(int id)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                using (var db = new DefaultConnection())
                {
                    var entity = db.Asset.Where(n => n.ID == id).FirstOrDefault();
                    if (entity == null)
                    {
                        return ResponseUtil.Error("记录不存在");
                    }
                    if (entity.Status != DEP_Constants.Asset_Status_Available)
                    {
                        return ResponseUtil.Error("当前资产不可报废");
                    }
                    entity.Status = DEP_Constants.Asset_Status_Scraped;
                    entity.UpdateTime = DateTime.Now;
                    // 记录
                    var record = new XYD_Asset_Record();
                    record.AssetID = entity.ID;
                    record.Operation = DEP_Constants.Asset_Status_Scraped;
                    record.EmplName = employee.EmplName;
                    record.DeptName = employee.DeptName;
                    record.CreateTime = DateTime.Now;
                    record.UpdateTime = DateTime.Now;
                    db.AssetRecord.Add(record);
                    db.SaveChanges();
                    return ResponseUtil.OK("报废成功");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 资产列表
        [Authorize]
        [HttpPost]
        public ActionResult List(XYD_Asset model, int Page = 0, int Size = 10)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var list = db.Asset.Where(n => true);
                    if (!string.IsNullOrEmpty(model.Category))
                    {
                        list = list.Where(n => n.Category == model.Category);
                    }
                    if (!string.IsNullOrEmpty(model.Status))
                    {
                        list = list.Where(n => n.Status == model.Status);
                    }
                    if (!string.IsNullOrEmpty(model.Name))
                    {
                        list.Where(n => n.Name.Contains(model.Name));
                    }
                    if (!string.IsNullOrEmpty(model.Area))
                    {
                        list.Where(n => n.Area == model.Area);
                    }
                    // 记录总数
                    var totalCount = list.Count();
                    // 记录总页数
                    var totalPage = (int)Math.Ceiling((float)totalCount / Size);
                    var results = list.OrderByDescending(n => n.CreateTime).Skip(Page * Size).Take(Size).ToList();
                    foreach(var asset in results)
                    {
                        var status = string.Empty;
                        if (asset.Status == DEP_Constants.Asset_Status_Available)
                        {
                            status = "可申领";
                        } else if (asset.Status == DEP_Constants.Asset_Status_Used)
                        {
                            status = "已申领";
                        } else
                        {
                            status = "已报废";
                        }
                        asset.Status = status;
                    }
                    return ResponseUtil.OK(new {
                        records = results,
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
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 资产操作记录
        [Authorize]
        public ActionResult Records(int id, int Page = 0, int Size = 10)
        {
            try
            {
                var results = new List<object>();
                var db = new DefaultConnection();

                var records = db.AssetRecord.Where(n => n.AssetID == id).OrderBy(n => n.ID);
                // 记录总数
                var totalCount = records.Count();
                // 记录总页数
                var totalPage = (int)Math.Ceiling((float)totalCount / Size);
                var list = records.Skip(Page * Size).Take(Size).ToList();
                foreach (var record in list)
                {
                    var result = new
                    {
                        ID = record.ID,
                        Asset = db.Asset.Where(n => n.ID == record.AssetID).FirstOrDefault().Name,
                        Operation = record.Operation,
                        EmplName = record.EmplName,
                        DeptName = record.DeptName,
                        CreateTime = record.CreateTime,
                        UpdateTime = record.UpdateTime
                    };
                    results.Add(result);
                }
                return ResponseUtil.OK(new {
                    records = results,
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
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 可申领物品列表
        [Authorize]
        public ActionResult AvailableAssets()
        {
            try
            {
                var AssetImage = System.Configuration.ConfigurationManager.AppSettings["AssetImage"];

                using (var db = new DefaultConnection())
                {
                    var assets = db.Asset.Where(n => n.Status == DEP_Constants.Asset_Status_Available)
                        .GroupBy(n => n.Name)
                        .Select(n => new
                        {
                            Name = n.FirstOrDefault().Name,
                            Image = AssetImage,
                            Count = n.Count()
                        }).ToList();
                    return ResponseUtil.OK(assets);
                }
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 判断用户是否具有领导权限
        [Authorize]
        public ActionResult CheckLeader()
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                var isLeader = PermUtil.CheckPermission(employee.EmplID, DEP_Constants.Module_Asset_Code, DEP_Constants.Perm_Asset_Leader);
                return ResponseUtil.OK(new
                {
                    isLeader = isLeader
                });
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 统计资产数目
        [Authorize]
        public ActionResult Summary(string area, string status)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var assets = db.Asset.Where(n => n.Area == area && n.Status == status)
                        .GroupBy(n => new { n.Name, n.Model })
                        .Select(n => new
                        {
                            Name = n.FirstOrDefault().Name,
                            Model = n.FirstOrDefault().Model,
                            Unit = n.FirstOrDefault().Unit,
                            Price = n.Sum(x => x.UnitPrice),
                            Count = n.Count()
                        }).ToList();
                    var totalCount = assets.Sum(n => n.Count);
                    return ResponseUtil.OK(new {
                        assets = assets,
                        totalCount = totalCount
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