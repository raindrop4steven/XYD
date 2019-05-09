using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DeptOA.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("/Apps/DEP/index.html#/index");
        }

        public ActionResult DetailPage(string mid)
        {
            return PartialView("gw_html");
        }
    }
}