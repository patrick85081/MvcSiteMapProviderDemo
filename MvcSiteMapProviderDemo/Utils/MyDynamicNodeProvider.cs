using System;
using System.Collections.Generic;
using System.Linq;
using MvcSiteMapProvider;
using MvcSiteMapProviderDemo.Models;

namespace MvcSiteMapProviderDemo.Utils
{
    public class MyDynamicNodeProvider : DynamicNodeProviderBase
    {
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection(ISiteMapNode node)
        {
            using (var context = new WebSiteContext())
            {
                context.Database.Log = System.Console.WriteLine;
                var loginUserId = "Foo";
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
                           RouteValues = routeValue
                       };
            }
        }
    }
}