﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using MvcSiteMapProvider.Loader;
using MvcSiteMapProviderDemo.ViewModels;
using Newtonsoft.Json;

namespace MvcSiteMapProviderDemo
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            DatabaseConfig.Init();
            UnityConfig.GetConfiguredContainer();
        }

        void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (Request.IsAuthenticated)
            {
                // 先取得該使用者的 FormsIdentity
                FormsIdentity id = (FormsIdentity)User.Identity;
                // 再取出使用者的 FormsAuthenticationTicket
                FormsAuthenticationTicket ticket = id.Ticket;
                // 將儲存在 FormsAuthenticationTicket 中的角色定義取出，並轉成字串陣列
                //string[] roles = ticket.UserData.Split(new char[] { ',' });
                string[] roles = JsonConvert.DeserializeObject<UserInfoViewModel>(ticket.UserData).Roles;
                // 指派角色到目前這個 HttpContext 的 User 物件去
                Context.User = new GenericPrincipal(Context.User.Identity, roles);
            }
        }
    }
}
