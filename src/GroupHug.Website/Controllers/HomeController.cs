using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GroupHug.Website.Services;

namespace GroupHug.Website.Controllers {
    public class HomeController : Controller {

        private const string imageUrlFormat =
            "http://res.cloudinary.com/{0}/image/upload/w_256,h_256,c_thumb,g_face/{1}.jpg";
        private Account cloudinaryAccount = new Account {
            ApiKey = "631184585298337",
            ApiSecret = "zCcMvsiHsJWU3jbjejOoZhyWDyk",
            Cloud = "dnj4vtvoq"
        };

        public ActionResult Index() {
            return View();
        }
        public ActionResult People() {
            return View();
        }

        public ActionResult Me() {
            var employee = (Employee)User.Identity;
            employee.ImageUrl = String.Format(imageUrlFormat, cloudinaryAccount.Cloud, employee.SamAccountName);
            return (View(employee));
        }

        public ActionResult UPload(HttpPostedFileBase file) {
            var tempFilePath = Path.Combine(Path.GetTempPath(),
                Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
            file.SaveAs(tempFilePath);

            // TODO: this doesn't work - we can't reuse the public ID. Need to 
            // capture a fresh one each time and persist it alongside the employee
            // info somewhere - either in AD or in a local/synced database.
            // Ho hum. :/
            var uploadParams = new ImageUploadParams() {
                File = new FileDescription(tempFilePath),
                PublicId = ((Employee)User.Identity).SamAccountName,
                Invalidate = true
            };

            var cloudinary = new Cloudinary(cloudinaryAccount);
            var result = cloudinary.Upload(uploadParams);
            return (RedirectToAction("Me"));
        }
    }
}
