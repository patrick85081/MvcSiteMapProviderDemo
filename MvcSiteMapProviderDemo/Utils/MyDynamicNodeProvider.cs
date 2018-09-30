using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using MvcSiteMapProvider;
using MvcSiteMapProviderDemo.Models;
using MvcSiteMapProviderDemo.ViewModels;
using Newtonsoft.Json;

namespace MvcSiteMapProviderDemo.Utils
{
    public class MyDynamicNodeProvider : DynamicNodeProviderBase
    {
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection(ISiteMapNode node)
        {
            using (var context = new WebSiteContext())
            {
                context.Database.Log = System.Console.WriteLine;

                if (!HttpContext.Current.Request.IsAuthenticated)
                    return Enumerable.Empty<DynamicNode>();
                
                //var loginUserId = "Foo";
                var loginUserId = UserInfoViewModel.GetCurrent().UserId;
                var roleMenus = (from user in context.Users
                                 where user.UserId == loginUserId
                                 from role in user.Roles
                                 from menu in role.Menus
                                 select menu)
                    .Distinct()
                    .ToList();

                return from menu in roleMenus
                       let routeValue = (menu?.RouteValues ?? "")
                           .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                           .Select(item => item.Split('='))
                           .ToDictionary(k => k[0], v => (object)v[1])
                       select new DynamicNode
                       {
                           Key = menu.MenuId.ToString(),
                           Title = menu.Name,
                           ParentKey = menu.ParentId.HasValue ? menu.ParentId.Value.ToString() : "",
                           Action = menu.Action,
                           Controller = menu.Controller,
                           Url = menu.Url,
                           RouteValues = routeValue ,
                           //Roles = menu.Roles.Select(r => r.Name).ToArray()
                       };
            }
        }
    }
}