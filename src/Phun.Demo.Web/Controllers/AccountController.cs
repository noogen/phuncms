using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using Phun.Demo.Web.Models;

namespace Phun.Demo.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        //
        // Automatically log you in on any page
        // GET: /Account/LoginAsAdmin

        [AllowAnonymous]
        public ActionResult LoginAsAdmin()
        {
            FormsAuthentication.RedirectFromLoginPage("Phun Admin", false);
            return this.Redirect("/");
        }

        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }
    }
}
