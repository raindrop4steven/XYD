using System;
using System.Web.Mvc;

namespace XYD.Controllers
{
    public class TestController : Controller
    {
        public ActionResult caculate()
        {
            throw new Exception("that's cool");
        }
    }
}