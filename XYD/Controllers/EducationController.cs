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
    public class EducationController : Controller
    {
        #region 添加
        [Authorize]
        public ActionResult Add(XYD_Education model)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    model.EmplID = model.EmplID;
                    model.CreateTime = DateTime.Now;
                    model.UpdateTime = DateTime.Now;
                    db.Education.Add(model);
                    db.SaveChanges();
                    return ResponseUtil.OK("添加成功");
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 编辑
        [Authorize]
        public ActionResult Update(XYD_Education model)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var entity = db.Education.Where(n => n.ID == model.ID).FirstOrDefault();
                    entity.School = model.School;
                    entity.StartDate = model.StartDate;
                    entity.EndDate = model.EndDate;
                    entity.Level = model.Level;
                    entity.Major = model.Major;
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
                    var entity = db.Education.Where(n => n.ID == id).FirstOrDefault();
                    if (entity == null)
                    {
                        return ResponseUtil.Error("未找到该条记录");
                    }
                    db.Education.Remove(entity);
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
                    var education = db.Education.Where(n => n.ID == id).FirstOrDefault();
                    if (education == null)
                    {
                        return ResponseUtil.Error("未找到该条记录");
                    }
                    return ResponseUtil.OK(education);
                }
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 学历列表
        [Authorize]
        public ActionResult LevelList()
        {
            return ResponseUtil.OK(new List<object> {
                new
                {
                    key = DEP_Constants.Education_Level_Bo,
                    value = DEP_Constants.Education_Level_Bo
                },
                new
                {
                    key = DEP_Constants.Education_Level_Shuo,
                    value = DEP_Constants.Education_Level_Shuo
                },
                new
                {
                    key = DEP_Constants.Education_Level_Ben,
                    value = DEP_Constants.Education_Level_Ben
                },
                new
                {
                    key = DEP_Constants.Education_Level_Zhuan,
                    value = DEP_Constants.Education_Level_Zhuan
                }
            });
        }
        #endregion
    }
}