using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GroupHug.Website.Controllers {
    public class HomeController : Controller {
        //
        // GET: /Home/

        public ActionResult People() {
            return View();
        }

    }
}
