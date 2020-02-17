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
        [Authorize]
        public ActionResult Add(XYD_MettingBook model)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;

                using (var db = new DefaultConnection())
                {
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
        [Authorize]
        public ActionResult CheckMeetingBook(XYD_MettingBook model)
        {
            try
            {
                var employee = (User.Identity as AppkizIdentity).Employee;

                using (var db = new DefaultConnection())
                {
                    // 检查该会议室是否被占用
                    var book = db.MettingBook.Where(n => n.Area == model.Area && n.MeetingRoom == model.MeetingRoom && (n.StartTime <= model.EndTime && model.StartTime <= n.EndTime)).FirstOrDefault();
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

        #region 会议室统计
        [Authorize]
        public ActionResult MeetingRecord(XYD_MettingBook model, int Page = 0, int Size = 10)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var records = db.MettingBook.Where(n => true);
                    if (model.Area != DEP_Constants.System_Config_Area_All)
                    {
                        records = records.Where(n => n.Area == model.Area);
                    }
                    if (!string.IsNullOrEmpty(model.Name))
                    {
                        records = records.Where(n => n.Name.Contains(model.Name));
                    }
                    if (model.StartTime != null)
                    {
                        records = records.Where(n => n.StartTime >= model.StartTime);
                    }
                    if (model.EndTime != null)
                    {
                        records = records.Where(n => n.EndTime <= model.EndTime);
                    }
                    // 记录总数
                    var totalCount = records.Count();
                    // 记录总页数
                    var totalPage = (int)Math.Ceiling((float)totalCount / Size);
                    var results = records.OrderByDescending(n => n.CreateTime).Skip(Page * Size).Take(Size).ToList();
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

        #region 会议室列表
        [Authorize]
        public ActionResult MeetingRooms(string Area)
        {
            try
            {
                var meetingRoomKey = string.Format("{0}_MeetingRoom", Area);
                var meetingRoom = System.Configuration.ConfigurationManager.AppSettings[meetingRoomKey];
                var rooms = meetingRoom.Split(',').Select(n => n.Trim()).ToList();
                return ResponseUtil.OK(new
                {
                    rooms = rooms
                });
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 会议室日统计
        [Authorize]
        public ActionResult DayData(string Area, DateTime Date)
        {
            /*
             * 参数定义
             */
            // 会议室预定结果
            Dictionary<string, List<object>> dayData = new Dictionary<string, List<object>>();
            try
            {
                // 或得日期对应的开始，结束日期
                var startDateTime = CommonUtils.StartOfDay(Date);
                var endDateTime = CommonUtils.EndOfDay(Date);
                using (var db = new DefaultConnection())
                {
                    var books = db.MettingBook.Where(n => n.StartTime >= startDateTime && n.EndTime < endDateTime && n.Area == Area).OrderBy(n => n.MeetingRoom).ToList();
                    foreach(var book in books)
                    {
                        var key = book.MeetingRoom;
                        var value = new List<object>();
                        if (dayData.ContainsKey(key))
                        {
                            value = dayData[key];
                        }
                        value.Add(book);
                        dayData[key] = value;
                    }
                    return ResponseUtil.OK(new
                    {
                        dayData = dayData
                    });
                }
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 周会议预定统计
        [Authorize]
        public ActionResult WeekData(string Area, DateTime BeginDate, DateTime EndDate)
        {
            try
            {
                var weekData = new List<object>();
                for (var day = BeginDate.Date; day.Date <= EndDate.Date; day = day.AddDays(1))
                {
                    // 或得日期对应的开始，结束日期
                    var startDateTime = CommonUtils.StartOfDay(day);
                    var endDateTime = CommonUtils.EndOfDay(day);
                    using (var db = new DefaultConnection())
                    {
                        var books = db.MettingBook.Where(n => n.StartTime >= startDateTime && n.EndTime < endDateTime && n.Area == Area).OrderBy(n => n.StartTime).ToList();
                        weekData.Add(new
                        {
                            week = day.DayOfWeek,
                            books = books 
                        });
                    }
                }

                return ResponseUtil.OK(new
                {
                    weekData = weekData
                });
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion
    }

}