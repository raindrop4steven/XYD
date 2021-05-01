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
                    db.SaveChanges();
                    return ResponseUtil.OK("假期设定添加成功");
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