using System.Web.Mvc;

namespace XYD.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("/Apps/XYD/index.html#/index");
        }
    }
}