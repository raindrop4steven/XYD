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
    public class AwardController : Controller
    {
        #region 添加
        [Authorize]
        public ActionResult Add(XYD_Award model)
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
                    db.Award.Add(model);
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
        public ActionResult Update(XYD_Award model)
        {
            try
            {
                // 当前用户
                var employee = (User.Identity as AppkizIdentity).Employee;
                using (var db = new DefaultConnection())
                {
                    var entity = db.Award.Where(n => n.ID == model.ID).FirstOrDefault();
                    if (entity == null)
                    {
                        return ResponseUtil.Error("未找到该条记录");
                    }
                    entity.Name = model.Name;
                    entity.Attachment = model.Attachment;
                    entity.UpdateTime = DateTime.Now;
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

        #region 删除紧急联系人
        [Authorize]
        public ActionResult Delete(int id)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var entity = db.Award.Where(n => n.ID == id).FirstOrDefault();
                    if (entity == null)
                    {
                        return ResponseUtil.Error("未找到该条记录");
                    }
                    db.Award.Remove(entity);
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
                    var entity = db.Award.Where(n => n.ID == id).FirstOrDefault();
                    if (entity == null)
                    {
                        return ResponseUtil.Error("未找到该联系人");
                    }
                    return ResponseUtil.OK(new
                    {
                        ID = entity.ID,
                        Name = entity.Name,
                        Attachment = db.Attachment.ToList().Where(n => entity.Attachment.Split(',').Select(int.Parse).ToList().Contains(n.ID)).Select(n => new
                        {
                            id = n.ID,
                            name = n.Name,
                            url = Url.Action("Download", "Common", new { id = n.ID })
                        })
                    });
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