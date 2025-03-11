using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using yazilimMuhProje.Repository;

namespace yazilimMuhProje.Controllers
{
    public class HomeController : Controller
    {
        ResimlerRepository resimRepo = new ResimlerRepository();

        public ActionResult Index()
        {
            var resimler = resimRepo.list();
            return View(resimler);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }
    }
}