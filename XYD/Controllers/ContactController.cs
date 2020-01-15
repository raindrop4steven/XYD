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
    public class ContactController : Controller
    {
        #region 添加紧急联系人
        [Authorize]
        public ActionResult Add(XYD_Contact model)
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
                    db.Contact.Add(model);
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

        #region 编辑紧急联系人
        [Authorize]
        public ActionResult Update(XYD_Contact model)
        {
            try
            {
                // 当前用户
                var employee = (User.Identity as AppkizIdentity).Employee;
                using (var db = new DefaultConnection())
                {
                    var contact = db.Contact.Where(n => n.ID == model.ID).FirstOrDefault();
                    if (contact == null)
                    {
                        return ResponseUtil.Error("未找到对应联系人");
                    }
                    contact.Name = model.Name;
                    contact.Contact = model.Contact;
                    contact.UpdateTime = DateTime.Now;
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

        #region 删除紧急联系人
        [Authorize]
        public ActionResult Delete(int id)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var contact = db.Contact.Where(n => n.ID == id).FirstOrDefault();
                    if (contact == null)
                    {
                        return ResponseUtil.Error("未找到该联系人");
                    }
                    db.Contact.Remove(contact);
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

        #region 详情
        [Authorize]
        public ActionResult Detail(int id)
        {
            try
            {
                using (var db = new DefaultConnection())
                {
                    var contact = db.Contact.Where(n => n.ID == id).FirstOrDefault();
                    if (contact == null)
                    {
                        return ResponseUtil.Error("未找到该联系人");
                    }
                    return ResponseUtil.OK(contact);
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