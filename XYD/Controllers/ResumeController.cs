using Appkiz.Library.Security;
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
    public class ResumeController : Controller
    {
        // 用户管理
        OrgMgr orgMgr = new OrgMgr();


        #region 用户简历信息
        public ActionResult Info()
        {
            // 用户基本信息
            var employee = (User.Identity as AppkizIdentity).Employee;
            using(var db = new DefaultConnection())
            {
                // 联系人
                var contacts = db.Contact.Where(n => n.EmplID == employee.EmplID).OrderByDescending(n => n.UpdateTime).ToList();
                // 工作经历
                var experiences = db.WorkExperience.Where(n => n.EmplID == employee.EmplID).OrderByDescending(n => n.StartDate).ToList();
                // 教育经历
                var educations = db.Education.Where(n => n.EmplID == employee.EmplID).OrderByDescending(n => n.StartDate).ToList();
                // 证书情况
                var awards = db.Award.Where(n => n.EmplID == employee.EmplID).OrderBy(n => n.CreateTime).ToList();

                return ResponseUtil.OK(new {
                    baseInfo = employee,
                    contacts = contacts,
                    experiences = experiences,
                    educations = educations,
                    awards = awards
                });
            }
        }
        #endregion
    }
}