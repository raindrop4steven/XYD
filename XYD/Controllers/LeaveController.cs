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
    public class LeaveController : Controller
    {
        #region 添加请假记录
        public ActionResult Add(XYD_Leave_Record model)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;
                model.EmplID = employee.EmplID;
                model.CreateTime = DateTime.Now;
                model.UpdateTime = DateTime.Now;
                model.Approved = false;
                using (var db = new DefaultConnection())
                {
                    db.LeaveRecord.Add(model);
                    db.SaveChanges();
                    return ResponseUtil.OK("添加请假记录成功");
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