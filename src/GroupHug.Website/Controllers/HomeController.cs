using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GroupHug.Website.Services;

namespace GroupHug.Website.Controllers {
    public class HomeController : Controller {

        private const string imageUrlFormat =
            "http://res.cloudinary.com/{0}/image/upload/w_256,h_256,c_thumb,g_face/{1}.jpg";
        private const string thumbnailPhotoUrlFormat =
            "http://res.cloudinary.com/{0}/image/upload/w_96,h_96,c_thumb,g_face/{1}.jpg";
        private Account cloudinaryAccount = new Account {
            ApiKey = "631184585298337",
            ApiSecret = "zCcMvsiHsJWU3jbjejOoZhyWDyk",
            Cloud = "dnj4vtvoq"
        };

        public ActionResult Index() {
            return View();
        }
        public ActionResult People() {
            var ad = new ActiveDirectoryServer();
            var employees = ad.ListEmployees();
            foreach (var employee in employees) {
                employee.ImageUrl = String.Format(imageUrlFormat, cloudinaryAccount.Cloud, employee.Photo);
            }
            return View(employees);
        }

        public ActionResult Me() {
            var employee = (Employee)User.Identity;
            employee.ImageUrl = String.Format(imageUrlFormat, cloudinaryAccount.Cloud, employee.Photo);
            return (View(employee));
        }

        public ActionResult Upload(HttpPostedFileBase file) {
            var tempFilePath = Path.Combine(Path.GetTempPath(),
                Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
            file.SaveAs(tempFilePath);

            // TODO: this doesn't work - we can't reuse the public ID. Need to 
            // capture a fresh one each time and persist it alongside the employee
            // info somewhere - either in AD or in a local/synced database.
            // Ho hum. :/
            var publicId = String.Format("{0}.{1}", ((Employee)User.Identity).SamAccountName, DateTime.Now.ToString("yyyyMMddhhmmssff"));
            var uploadParams = new ImageUploadParams() {
                File = new FileDescription(tempFilePath),
                PublicId = publicId,
            };

            var cloudinary = new Cloudinary(cloudinaryAccount);
            var result = cloudinary.Upload(uploadParams);

            var principalContext = new PrincipalContext(ContextType.Domain);
            var user = UserPrincipal.FindByIdentity(principalContext, User.Identity.Name);
            var directoryEntry = user.GetUnderlyingObject() as DirectoryEntry;

            directoryEntry.Properties["photo"].Value = result.PublicId;

            var thumbnailPhotoUrl = String.Format(thumbnailPhotoUrlFormat, cloudinaryAccount.Cloud, result.PublicId);
            using (var wc = new WebClient()) {
                var thumbnailPhotoAsBytes = wc.DownloadData(thumbnailPhotoUrl);
                directoryEntry.Properties["thumbnailPhoto"].Value = thumbnailPhotoAsBytes;
                var jpegPhotoUrl = String.Format(imageUrlFormat, cloudinaryAccount.Cloud, result.PublicId);
                var jpegPhotoAsBytes = wc.DownloadData(jpegPhotoUrl);
                directoryEntry.Properties["jpegPhoto"].Value = jpegPhotoAsBytes;
            }
            directoryEntry.CommitChanges();
            return (RedirectToAction("Me"));
        }
    }
}
