using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MvcSiteMapProvider.Web.Mvc.Filters;
using MvcSiteMapProviderDemo.ViewModels;
using MvcSiteMapProviderDemo.Models;
using Newtonsoft.Json;

namespace MvcSiteMapProviderDemo.Controllers
{
    public class UserController : Controller
    {
        private readonly WebSiteContext db = new WebSiteContext();

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        [AuthorizeFromSql]
        public ActionResult Edit() => View();

        public class AuthorizeFromSqlAttribute : AuthorizeAttribute
        {
            public override void OnAuthorization(AuthorizationContext filterContext)
            {
                if (!filterContext.HttpContext.Request.IsAuthenticated)
                {
                    filterContext.Result = new HttpUnauthorizedResult();
                    return;
                }

                var menuRole = GetMenuRole();
                if (menuRole == null)
                {
                    base.OnAuthorization(filterContext);
                    return;
                }

                var controllerName = filterContext.ActionDescriptor
                    .ControllerDescriptor
                    .ControllerName
                    .Replace("Controller", "");
                var actionName = filterContext.ActionDescriptor
                    .ActionName;

                var matchMenus = menuRole.Where(mr => mr.Controller == controllerName);
                var menuRule = matchMenus.FirstOrDefault(m => m.Action == actionName) ??
                                    matchMenus.FirstOrDefault(m => string.IsNullOrEmpty(m.Action));

                if (menuRule == null)
                {
                    base.OnAuthorization(filterContext);
                    return;
                }
                else
                {

                    if (menuRule.Controller != controllerName && 
                        (!string.IsNullOrEmpty(menuRule.Action) || actionName != menuRule.Action))
                    {
                        filterContext.Result = new HttpUnauthorizedResult();
                    }
                }
            }

            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                GetMenuRole();

                return base.AuthorizeCore(httpContext);
            }

            private static MenuRoleModel[] GetMenuRole()
            {
                var objectCache = MemoryCache.Default;

                if (!objectCache.Contains("MenuRole"))
                {
                    var db = new WebSiteContext();
                    var menuRoles = db.Menus.Include("Roles")
                        .Where(m => !string.IsNullOrEmpty(m.Controller)) //&& !string.IsNullOrEmpty(m.Action))
                        .ToArray()
                        .Select(m => new MenuRoleModel
                        {
                            Controller = m.Controller,
                            Action = m.Action,
                            Roles = m.Roles.Select(r => r.Name).ToArray()
                        })
                        .ToArray();
                    objectCache.Add("MenuRole", menuRoles,
                        new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.AddDays(1) });
                }

                MenuRoleModel[] roles = objectCache["MenuRole"] as MenuRoleModel[];
                return roles;
            }

            public class MenuRoleModel
            {
                public string Controller { get; set; }
                public string Action { get; set; }
                public string[] Roles { get; set; }
            }
        }

        public ActionResult Login()
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [SiteMapCacheRelease]
        public ActionResult Login(LoginViewModel login)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = string.Join(", ",
                    ModelState.Values
                        .Where(v => v.Errors.Any())
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                return View(login);
            }

            var user = db.Users.FirstOrDefault(u => u.Email == login.Email && u.Password == login.Password);
            if (user == null)
            {
                ViewBag.ErrorMessage = "帳號密碼錯誤 ！！";
                return View(login);
            }

            Session.Clear();
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                version: 1,
                name: user.Name,
                issueDate: DateTime.Now,
                expiration: DateTime.Now.AddDays(1),
                isPersistent: false,
                userData: JsonConvert.SerializeObject(new UserInfoViewModel
                { UserId = user.UserId, UserName = user.Name, Roles = user.Roles.Select(r => r.Name).ToArray() }),
                cookiePath: FormsAuthentication.FormsCookiePath);

            var encTicket = FormsAuthentication.Encrypt(ticket);
            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
            return RedirectToAction("Index", "Home");
        }

        [SiteMapCacheRelease]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            db.Dispose();
        }
    }
}