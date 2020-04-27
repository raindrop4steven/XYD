using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Models;

namespace XYD.Controllers
{
    public class BiztripController : Controller
    {
        #region 添加出差记录
        public ActionResult Add(XYD_BizTrip model, string user, string mid)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var leave = db.BizTrip.Where(n => n.EmplID == user && n.MessageID == mid).FirstOrDefault();
                    if (leave == null)
                    {
                        model.EmplID = user;
                        model.MessageID = mid;
                        model.CreateTime = DateTime.Now;
                        model.UpdateTime = DateTime.Now;
                        db.BizTrip.Add(model);
                    }
                    else
                    {
                        leave.UpdateTime = DateTime.Now;
                        leave.StartDate = model.StartDate;
                        leave.EndDate = model.EndDate;
                    }
                    db.SaveChanges();
                    return ResponseUtil.OK("添加出差记录成功");
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