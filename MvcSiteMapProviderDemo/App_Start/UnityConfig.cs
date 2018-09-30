using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Practices.Unity;
using MvcSiteMapProvider.Loader;
using MvcSiteMapProviderDemo.DI.Unity.ContainerExtensions;

namespace MvcSiteMapProviderDemo
{
    public class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }
        #endregion

        public static void RegisterTypes(IUnityContainer container)
        {
            // Add the extension module (required)
            container.AddNewExtension<MvcSiteMapProviderContainerExtension>();

            // Setup global sitemap loader (required)
            MvcSiteMapProvider.SiteMaps.Loader = container.Resolve<ISiteMapLoader>();
        }
    }
}