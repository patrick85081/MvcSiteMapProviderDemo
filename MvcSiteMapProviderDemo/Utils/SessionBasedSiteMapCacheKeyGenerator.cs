using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.Web.Mvc;

namespace MvcSiteMapProviderDemo.Utils
{
    public class SessionBasedSiteMapCacheKeyGenerator : ISiteMapCacheKeyGenerator
    {
        // fields
        protected readonly IMvcContextFactory mvcContextFactory;

        // constructor
        public SessionBasedSiteMapCacheKeyGenerator(IMvcContextFactory mvcContextFactory)
        {
            if (mvcContextFactory == null)
                throw new ArgumentNullException("mvcContextFactory");
            this.mvcContextFactory = mvcContextFactory;
        }

        // methods - ISiteMapCacheKeyGenerator Members
        public virtual string GenerateKey()
        {
            var context = mvcContextFactory.CreateHttpContext();
            var builder = new StringBuilder();
            builder.Append("sitemap://");
            builder.Append(context.Request.Url.DnsSafeHost);
            builder.Append("/?sessionId=");
            builder.Append(context.Session.SessionID);

            //LogUtility.Logger.Debug($"key = {builder.ToString()}");

            return builder.ToString();
        }
    }
}