using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Models;

namespace XYD.Controllers
{
    public class SystemController : Controller
    {
        #region 展示地区配置
        [Authorize]
        public ActionResult ShowConfig(string areaKey)
        {
            try
            {
                var db = new DefaultConnection();
                var config = db.SystemConfig.Where(n => n.Area == areaKey).FirstOrDefault();
                var banners = new List<int>();
                if (!string.IsNullOrEmpty(config.Banners))
                {
                    banners = config.Banners.Split(',').Select(int.Parse).ToList();
                    
                }
                var atts = db.Attachment.Where(n => banners.Contains(n.ID)).ToList().Select(n => new
                {
                    id = n.ID,
                    name = n.Name,
                    path = Url.Action("Download", "Common", new { id = n.ID })
                });

                return ResponseUtil.OK(new
                {
                    ID = config.ID,
                    Area = config.Area,
                    StartWorkTime = config.StartWorkTime,
                    EndWorkTime = config.EndWorkTime,
                    RestStartTime = config.RestStartTime,
                    RestEndTime = config.RestEndTime,
                    RestDays = config.RestDays,
                    Allowance = config.Allowance,
                    Banners = atts
                });
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 更新地区配置
        [Authorize]
        public ActionResult UpdateConfig(XYD_System_Config model)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var configs = db.SystemConfig.ToList();
                    foreach(var config in configs)
                    {
                        if (config.Area == model.Area)
                        {
                            config.StartWorkTime = model.StartWorkTime;
                            config.EndWorkTime = model.EndWorkTime;
                            //config.RestDays = model.RestDays;
                            //config.Allowance = model.Allowance;
                            config.RestStartTime = model.RestStartTime;
                            config.RestEndTime = model.RestEndTime;
                        }
                        config.Banners = model.Banners;
                    }
                    
                    db.SaveChanges();
                    return ResponseUtil.OK("更新成功");
                }
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 地区列表
        [Authorize]
        public ActionResult AreaList()
        {
            return ResponseUtil.OK(new List<object> {
                new
                {
                    key = DEP_Constants.System_Config_Area_SH,
                    value = DEP_Constants.System_Config_Name_SH
                },
                new
                {
                    key = DEP_Constants.System_Config_Area_WX,
                    value = DEP_Constants.System_Config_Name_WX
                }
            });
        }
        #endregion

        #region 轮播图列表
        [Authorize]
        public ActionResult BannerList()
        {
            try
            {
                var db = new DefaultConnection();
                var config = db.SystemConfig.FirstOrDefault();
                var banners = new List<int>();
                if (!string.IsNullOrEmpty(config.Banners))
                {
                    banners = config.Banners.Split(',').Select(int.Parse).ToList();
                }
                var atts = db.Attachment.Where(n => banners.Contains(n.ID)).ToList().Select(n => new
                {
                    id = n.ID,
                    name = n.Name,
                    path = Url.Action("Download", "Common", new { id = n.ID })
                });
                return ResponseUtil.OK(atts);
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region app自动更新
        public ActionResult CheckVersion()
        {
            // Package
            var name = Request.Params["name"];
            string AppVersion = System.Configuration.ConfigurationManager.AppSettings["AppVersion"];
            string AppUrl = System.Configuration.ConfigurationManager.AppSettings["Appurl"];
            string UpdateNote = System.Configuration.ConfigurationManager.AppSettings["UpdateNote"];

            var dict = new Dictionary<string, object>();
            var dictValue = new
            {
                Name = name,
                Type = "AndroidApp",
                Versions = new List<object>()
                {
                    new
                    {
                        Version = AppVersion,
                        Type = "R",
                        Note = UpdateNote,
                        Url = AppUrl,
                        Dependencies = new { }
                    }
                }
            };
            dict.Add(name, dictValue);

            return new JsonNetResult(dict);
        }
        #endregion
    }
}