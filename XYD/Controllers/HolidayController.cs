using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Models;

namespace XYD.Controllers
{
    public class HolidayController : Controller
    {
        #region 增加假期设定
        [Authorize]
        public ActionResult Add(XYD_Holiday model)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    db.Holiday.Add(model);
                    db.SaveChanges();
                    return ResponseUtil.OK("假期设定添加成功");
                }
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 更新假期设定
        [Authorize]
        public ActionResult Update(XYD_Holiday model)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var record = db.Holiday.Where(n => n.ID == model.ID).FirstOrDefault();
                    record.Name = model.Name;
                    record.StartDate = model.StartDate;
                    record.EndDate = model.EndDate;
                    record.Type = model.Type;
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

        #region 删除假期
        [Authorize]
        public ActionResult Delete(int ID)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var record = db.Holiday.Where(n => n.ID == ID).FirstOrDefault();
                    db.Holiday.Remove(record);
                    db.SaveChanges();
                    return ResponseUtil.OK("删除成功");
                }
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 假期列表
        [Authorize]
        public ActionResult List()
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var list = db.Holiday.OrderBy(n => n.StartDate).ToList();
                    return ResponseUtil.OK(new
                    {
                        list = list
                    });
                }
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 测试假期列表
        public ActionResult TestHoliday()
        {
            using (var db = new DefaultConnection())
            {
                var calendar = CalendarUtil.GetPlanByYear(2021);
                return ResponseUtil.OK(calendar);
            }
        }
        #endregion
    }
}