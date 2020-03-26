using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security.Authentication;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Models;

namespace XYD.Controllers
{
    public class AssetController : Controller
    {
        WorkflowMgr mgr = new WorkflowMgr();

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
                                XYD_Asset asset = new XYD_Asset();
                                asset.Name = name;
                                asset.Model = model;
                                asset.Count = count;
                                asset.Unit = unit;
                                asset.UnitPrice = unitPrice;
                                asset.Category = category;
                                asset.Area = area;
                                asset.CreateTime = DateTime.Now;
                                asset.UpdateTime = DateTime.Now;
                                list.Add(asset);
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

        #region 资产添加
        [Authorize]
        public ActionResult Add(XYD_Asset model)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                using (var db = new DefaultConnection())
                {
                    // 添加资产
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
        public ActionResult Apply(int id, int count)
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
                    if (entity.Count < count)
                    {
                        return ResponseUtil.Error("申领数量超过库存数量");
                    }
                    entity.Count -= count;
                    entity.UpdateTime = DateTime.Now;
                    // 记录
                    var record = new XYD_Asset_Record();
                    record.AssetID = entity.ID;
                    record.Count = count;
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
        public ActionResult Return(int id, int count)
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
                    entity.Count += count;
                    entity.UpdateTime = DateTime.Now;
                    // 记录
                    var record = new XYD_Asset_Record();
                    record.AssetID = entity.ID;
                    record.Count = count;
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

        #region 资产报废
        [Authorize]
        public ActionResult Scrap(int id, int count)
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
                    if (entity.Count < count)
                    {
                        return ResponseUtil.Error("报废数量超过库存");
                    }
                    entity.Count -= count;
                    entity.UpdateTime = DateTime.Now;
                    // 记录
                    var record = new XYD_Asset_Record();
                    record.AssetID = entity.ID;
                    record.Count = count;
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
                        // 固定资产有归还操作，所以数量为0的资产也可以显示
                        if (model.Category == DEP_Constants.ASSET_CATEGORY_CONSUME)
                        {
                            list = list.Where(n => n.Count > 0);
                        }
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
                var asset = db.Asset.Where(n => n.ID == id).FirstOrDefault();
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
                        Name = asset.Name,
                        Model = asset.Model,
                        Unit = asset.Unit,
                        Count = record.Count,
                        UnitPrice = asset.UnitPrice,
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
        public ActionResult AvailableAssets(string WorkflowId, int Page, int Size, bool? isWeb)
        {
            try
            {
                if (isWeb)
                {
                    Page -= 1;
                }
                var folderName = mgr.GetMessage(WorkflowId).Folder.FolderName;
                var area = folderName.Contains(DEP_Constants.System_Config_Name_WX) ? DEP_Constants.System_Config_Area_WX : DEP_Constants.System_Config_Area_SH;
                var AssetImage = System.Configuration.ConfigurationManager.AppSettings["AssetImage"];

                using (var db = new DefaultConnection())
                {
                    var assets = db.Asset.Where(n => n.Count > 0 && n.Area == area)
                        .GroupBy(n => new { n.Name, n.Model, n.Unit})
                        .Select(n => new
                        {
                            Name = n.FirstOrDefault().Name,
                            Model = n.FirstOrDefault().Model,
                            Unit = n.FirstOrDefault().Unit,
                            Image = AssetImage,
                            Count = n.Count()
                        });
                    // 记录总数
                    var totalCount = assets.Count();
                    // 记录总页数
                    var totalPage = (int)Math.Ceiling((float)totalCount / Size);
                    var results = assets.OrderByDescending(n => n.Name).Skip(Page * Size).Take(Size).ToList();
                    return ResponseUtil.OK(new
                    {
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
        public ActionResult Summary(string area, int Page, int Size)
        {
            try
            {
                var db = new DefaultConnection();
                // 当前资产列表
                var assets = db.Asset.Where(n => n.Area == area)
                        .GroupBy(n => new { n.Name, n.Model })
                        .Select(n => new
                        {
                            Name = n.FirstOrDefault().Name,
                            Model = n.FirstOrDefault().Model,
                            Unit = n.FirstOrDefault().Unit,
                            Price = n.Sum(x => x.UnitPrice * x.Count),
                            Count = n.Sum(x => x.Count)
                        });
                // 记录总数
                var totalCount = assets.Count();
                // 记录总页数
                var totalPage = (int)Math.Ceiling((float)totalCount / Size);
                var results = assets.OrderByDescending(n => n.Count).Skip(Page * Size).Take(Size).ToList();
                var currentCount = assets.Sum(n => n.Count);
                var usedCount = db.AssetRecord.Where(n => n.Operation == DEP_Constants.Asset_Operation_Apply).ToList().Sum(n => n.Count);
                var returnCount = db.AssetRecord.Where(n => n.Operation == DEP_Constants.Asset_Operation_Return).ToList().Sum(n => n.Count);
                var scrapedCount = db.AssetRecord.Where(n => n.Operation == DEP_Constants.Asset_Operation_Scrap).ToList().Sum(n => n.Count);
                return ResponseUtil.OK(new
                {
                    assets = results,
                    meta = new
                    {
                        current_page = Page,
                        total_page = totalPage,
                        current_count = Page * Size + results.Count(),
                        total_count = totalCount,
                        per_page = Size
                    },
                    currentCount = currentCount,
                    usedCount = usedCount,
                    returnCount = returnCount,
                    scrapedCount = scrapedCount
                });
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 资产统计下载
        [Authorize]
        public void Download(string area)
        {
            try
            {
                var db = new DefaultConnection();
                // 当前资产列表
                var assets = db.Asset.Where(n => n.Area == area)
                        .GroupBy(n => new { n.Name, n.Model })
                        .Select(n => new
                        {
                            Name = n.FirstOrDefault().Name,
                            Model = n.FirstOrDefault().Model,
                            Unit = n.FirstOrDefault().Unit,
                            Price = n.Sum(x => x.UnitPrice * x.Count),
                            Count = n.Sum(x => x.Count)
                        });
                var currentCount = assets.Sum(n => n.Count);
                var usedCount = db.AssetRecord.Where(n => n.Operation == DEP_Constants.Asset_Operation_Apply).ToList().Sum(n => n.Count);
                var returnCount = db.AssetRecord.Where(n => n.Operation == DEP_Constants.Asset_Operation_Return).ToList().Sum(n => n.Count);
                var scrapedCount = db.AssetRecord.Where(n => n.Operation == DEP_Constants.Asset_Operation_Scrap).ToList().Sum(n => n.Count);

                // 准备生成Excel文件
                // 1. 数据列表
                var titles = new List<string[]>() { new string[] { "物品名称", "物品型号", "单位", "总价", "数量" } };
                var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("资产统计");
                ws.Cell(1, 1).InsertData(titles);
                ws.Range(1, 1, 1, 5).AddToNamed("Titles");
                ws.Cell(2, 1).InsertData(assets);
                // 2. 统计数据
                var summaryTitles = new List<string[]>() { new string[] { "类别", "数量" } };
                ws.Cell(1, 7).InsertData(summaryTitles);
                ws.Range(1, 7, 1, 8).AddToNamed("Titles");
                var summaryData = new List<object>()
                {
                    new object[]
                    {
                        "可申领资产", currentCount
                    },
                    new object[]
                    {
                        "已申领数量", usedCount
                    },
                    new object[]
                    {
                        "已归还数量", returnCount
                    },
                    new object[]
                    {
                        "已报废数量", scrapedCount
                    }
                };
                ws.Cell(2, 7).InsertData(summaryData);
                // Prepare the style for the titles
                var titlesStyle = wb.Style;
                titlesStyle.Font.Bold = true;
                titlesStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titlesStyle.Fill.BackgroundColor = XLColor.Orange;

                // Format all titles in one shot
                wb.NamedRanges.NamedRange("Titles").Ranges.Style = titlesStyle;
                ws.Columns().AdjustToContents();
                string myName = Server.UrlEncode("资产统计" + "_" + DateTime.Now.ToShortDateString() + ".xlsx");
                MemoryStream stream = GetStream(wb);

                Response.Clear();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + myName);
                Response.ContentType = "application/vnd.ms-excel";
                Response.BinaryWrite(stream.ToArray());
                Response.End();
            }
            catch (Exception e)
            {
                
            }
        }

        public MemoryStream GetStream(XLWorkbook excelWorkbook)
        {
            MemoryStream fs = new MemoryStream();
            excelWorkbook.SaveAs(fs);
            fs.Position = 0;
            return fs;
        }
        #endregion
    }
}