using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Elasticsearch.Admin.Api;
using Umbraco.Web;
using Umbraco.Web.UI.JavaScript;

namespace Umbraco.Elasticsearch.Admin.Events
{
    public class SearchApplicationInstallServerVarsEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            InstallApiUrlServerVar();
        }

        private void InstallApiUrlServerVar()
        {
            ServerVariablesParser.Parsing += (sender, serverVars) =>
            {
                if (!serverVars.ContainsKey("umbracoUrls"))
                    throw new Exception("Missing umbracoUrls.");
                var umbracoUrlsObject = serverVars["umbracoUrls"];
                if (umbracoUrlsObject == null)
                    throw new Exception("Null umbracoUrls");
                var umbracoUrls = umbracoUrlsObject as Dictionary<string, object>;
                if (umbracoUrls == null)
                    throw new Exception("Invalid umbracoUrls");

                if (HttpContext.Current == null) throw new InvalidOperationException("HttpContext is null");
                var urlHelper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));

                umbracoUrls["umbElasticsearchApiUrl"] = urlHelper.GetUmbracoApiServiceBaseUrl<SearchApiController>(controller => controller.IndicesInfo());
            };
        }
    }
}
