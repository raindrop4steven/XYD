using Appkiz.Library.Security.Authentication;
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
        #region 资产添加
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
        public ActionResult CategoryList()
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                using (var db = new DefaultConnection())
                {
                    var list = db.AssetCategory.OrderBy(n => n.Order).ToList();
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
                        value = DEP_Constants.Asset_Status_Available
                    },
                    new
                    {
                        key = DEP_Constants.Asset_Status_Used,
                        value = DEP_Constants.Asset_Status_Used
                    },
                    new
                    {
                        key = DEP_Constants.Asset_Status_Scraped,
                        value = DEP_Constants.Asset_Status_Scraped
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
                    entity.Sn = model.Sn;
                    entity.Name = model.Name;
                    entity.Category = model.Category;
                    entity.Memo = model.Memo;
                    entity.Status = model.Status;
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
        [HttpPost]
        public ActionResult List(XYD_Asset model)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var list = db.Asset.Where(n => true);
                    if (model.Category != DEP_Constants.Asset_Category_All)
                    {
                        list = list.Where(n => n.Category == model.Category);
                    }
                    if (model.Status != DEP_Constants.Asset_Status_All)
                    {
                        list = list.Where(n => n.Status == model.Status);
                    }
                    if (!string.IsNullOrEmpty(model.Name))
                    {
                        list.Where(n => n.Name.Contains(model.Name));
                    }
                    var results = list.OrderByDescending(n => n.CreateTime).ToList();
                    return ResponseUtil.OK(results);
                }
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 资产操作记录
        public ActionResult Records(int id)
        {
            try
            {
                var results = new List<object>();
                var db = new DefaultConnection();
                var records = db.AssetRecord.Where(n => n.AssetID == id).OrderBy(n => n.ID);
                foreach (var record in records)
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
                return ResponseUtil.OK(results);
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 可申领物品列表
        public ActionResult AvailableAssets()
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var assets = db.Asset.Where(n => n.Status == DEP_Constants.Asset_Status_Available)
                        .GroupBy(n => n.Name)
                        .Select(n => new
                        {
                            Name = n.First().Name,
                            Count = n.Count().ToString()
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
    }
}