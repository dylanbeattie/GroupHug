using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GroupHug.Website.Services;

namespace GroupHug.Website.Controllers {
    public class HomeController : Controller {

        public ActionResult Index() {
            return View();
        }
        public ActionResult People() {
            return View();
        }

        public ActionResult Me() {
            return (View(User.Identity));
        }
    }
}
