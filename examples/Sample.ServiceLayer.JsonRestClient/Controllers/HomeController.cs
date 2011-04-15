using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sample.ServiceLayer.JsonRestClient.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewData["Message"] = "First Agatha REST Demo with new implementation";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
