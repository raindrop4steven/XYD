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
        public ActionResult ShowConfig(string areaKey)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var config = db.SystemConfig.Where(n => n.Area == areaKey).FirstOrDefault();
                    return ResponseUtil.OK(config);
                }
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 更新地区配置
        public ActionResult UpdateConfig(XYD_System_Config model)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var config = db.SystemConfig.Where(n => n.Area == model.Area).FirstOrDefault();
                    if (config == null)
                    {
                        return ResponseUtil.Error("记录不存在");
                    }
                    config.StartWorkTime = model.StartWorkTime;
                    config.EndWorkTime = model.EndWorkTime;
                    config.RestDays = model.RestDays;
                    config.Allowance = model.Allowance;
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
        public ActionResult AreaList()
        {
            return ResponseUtil.OK(new List<object> {
                new
                {
                    key = DEP_Constants.System_Config_Area_SH,
                    value = DEP_Constants.System_Config_Area_SH
                },
                new
                {
                    key = DEP_Constants.System_Config_Area_WX,
                    value = DEP_Constants.System_Config_Area_WX
                }
            });
        }
        #endregion

        #region 顶部广告列表
        public ActionResult BannerList()
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var banners = db.Banner.OrderBy(n => n.order).ToList();
                    return ResponseUtil.OK(banners);
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