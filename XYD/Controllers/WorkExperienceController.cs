using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Models;
using XYD.Common;
using Appkiz.Library.Security.Authentication;

namespace XYD.Controllers
{
    public class WorkExperienceController : Controller
    {
        #region 工作经历列表
        [Authorize]
        public ActionResult Add(XYD_WorkExperience model)
        {
            try
            {
                // 当前用户
                var employee = (User.Identity as AppkizIdentity).Employee;

                using (var db = new DefaultConnection())
                {
                    model.EmplID = employee.EmplID;
                    model.CreateTime = DateTime.Now;
                    model.UpdateTime = DateTime.Now;
                    db.WorkExperience.Add(model);
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

        #region 编辑工作经历
        [Authorize]
        public ActionResult Update(XYD_WorkExperience model)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var entity = db.WorkExperience.Where(n => n.ID == model.ID).FirstOrDefault();
                    entity.CompanyName = model.CompanyName;
                    entity.JobName = model.JobName;
                    entity.StartDate = model.StartDate;
                    entity.EndDate = model.EndDate;
                    entity.JobContent = model.JobContent;
                    entity.UpdateTime = DateTime.Now;
                    db.SaveChanges();
                    return ResponseUtil.OK("编辑成功");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 删除工作经历
        [Authorize]
        public ActionResult Delete(int id)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var experience = db.WorkExperience.Where(n => n.ID == id).FirstOrDefault();
                    if (experience == null)
                    {
                        return ResponseUtil.Error("未找到该条记录");
                    }
                    db.WorkExperience.Remove(experience);
                    db.SaveChanges();
                    return ResponseUtil.OK("删除成功");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 详情
        [Authorize]
        public ActionResult Detail(int id)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var experience = db.WorkExperience.Where(n => n.ID == id).FirstOrDefault();
                    if (experience == null)
                    {
                        return ResponseUtil.Error("未找到该条记录");
                    }
                    return ResponseUtil.OK(experience);
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