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
    public class MeetingBookController : Controller
    {
        #region 增加会议室预定
        public ActionResult Add(XYD_MettingBook model)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;

                using (var db = new DefaultConnection())
                {
                    // 检查该会议室是否被占用
                    var book = db.MettingBook.Where(n => n.Area == model.Area && n.MeetingRoom == model.MeetingRoom && (n.StartTime < model.StartTime || n.EndTime > model.EndTime)).FirstOrDefault();
                    if (book != null)
                    {
                        return ResponseUtil.Error("该会议室已被占用");
                    }
                    model.EmplID = employee.EmplID;
                    model.CreateTime = DateTime.Now;
                    model.UpdateTime = DateTime.Now;
                    db.MettingBook.Add(model);
                    db.SaveChanges();
                    return ResponseUtil.OK("预定记录添加成功");
                }
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 检查会议室是否可用
        public ActionResult CheckMeetingBook(XYD_MettingBook model)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;

                using (var db = new DefaultConnection())
                {
                    // 检查该会议室是否被占用
                    var book = db.MettingBook.Where(n => n.Area == model.Area && n.MeetingRoom == model.MeetingRoom && (n.StartTime <= model.StartTime || n.EndTime >= model.EndTime)).FirstOrDefault();
                    if (book != null)
                    {
                        return ResponseUtil.Error("该会议室已被占用");
                    } else
                    {
                        return ResponseUtil.OK("会议室可以使用");
                    }
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