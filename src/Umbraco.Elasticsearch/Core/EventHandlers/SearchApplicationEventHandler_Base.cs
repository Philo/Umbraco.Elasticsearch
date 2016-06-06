using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Dashboard;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Elasticsearch.Core.Admin.Api;
using Umbraco.Elasticsearch.Core.Config;
using Umbraco.Elasticsearch.Core.Impl;
using Umbraco.Web;
using Umbraco.Web.Cache;
using Umbraco.Web.UI.JavaScript;

namespace Umbraco.Elasticsearch.Core.EventHandlers
{
    public abstract class SearchApplicationEventHandler : SearchApplicationEventHandler<FromConfigSearchSettings>
    {
        protected SearchApplicationEventHandler() : base(new FromConfigSearchSettings())
        {
        }
    }

    public abstract partial class SearchApplicationEventHandler<TSearchSettings> : ApplicationEventHandler
        where TSearchSettings : ISearchSettings
    {
        protected SearchApplicationEventHandler(TSearchSettings searchSettings)
        {
            SearchSettings<TSearchSettings>.Set(searchSettings);

            ContentService.Published += ContentService_Published;
            ContentService.UnPublished += ContentServiceOnUnPublished;
            ContentService.Trashed += ContentServiceOnTrashed;
            CacheRefresherBase<PageCacheRefresher>.CacheUpdated += CacheRefresherBaseOnCacheUpdated;

            MediaService.Saved += MediaServiceOnSaved;
            MediaService.Deleted += MediaServiceOnDeleted;
        }

        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            InstallServerVars();
        }

        protected override void ApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            LogHelper.Info<SearchApplicationEventHandler<TSearchSettings>>(() => "Initialising configuration for Elasticsearch integration");
            Initialise(SearchSettings<TSearchSettings>.Current);
        }

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            foreach (var service in RegisterContentIndexingServices())
            {
                UmbracoSearchFactory.RegisterContentIndexService(service, service.ShouldIndex);
            }

            foreach (var service in RegisterMediaIndexingServices())
            {
                UmbracoSearchFactory.RegisterMediaIndexService(service, service.ShouldIndex);
            }

            /*
                var manager = new IndexManager();
                manager.Create(); 
            */
        }

        #region Event Handlers
        private void ContentServiceOnUnPublished(IPublishingStrategy sender, PublishEventArgs<IContent> publishEventArgs)
        {
            RemoveContentCore(publishEventArgs.PublishedEntities, publishEventArgs.Messages);
        }

        private void ContentServiceOnTrashed(IContentService sender, MoveEventArgs<IContent> moveEventArgs)
        {
            RemoveContentCore(moveEventArgs.MoveInfoCollection.Select(x => x.Entity), moveEventArgs.Messages);
        }

        private void ContentService_Published(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            IndexContentCore(e.PublishedEntities, e.Messages);
        }

        private void MediaServiceOnDeleted(IMediaService sender, DeleteEventArgs<IMedia> deleteEventArgs)
        {
            RemoveMediaCore(deleteEventArgs.DeletedEntities, deleteEventArgs.Messages);
        }

        private void MediaServiceOnSaved(IMediaService sender, SaveEventArgs<IMedia> saveEventArgs)
        {
            IndexMediaCore(saveEventArgs.SavedEntities, saveEventArgs.Messages);
        }

        private void CacheRefresherBaseOnCacheUpdated(PageCacheRefresher sender, CacheRefresherEventArgs cacheRefresherEventArgs)
        {
            var helper = new UmbracoHelper(UmbracoContext.Current);
            var contentService = helper.UmbracoContext.Application.Services.ContentService;

            IContent content = null;
            switch (cacheRefresherEventArgs.MessageType)
            {
                case MessageType.RefreshById:
                    content = contentService.GetById((int)cacheRefresherEventArgs.MessageObject);
                    break;
                case MessageType.RefreshByInstance:
                    content = cacheRefresherEventArgs.MessageObject as IContent;
                    break;
            }

            if (content != null)
            {
                if (content.CreateDate == content.UpdateDate)
                {
                    IndexContentCore(new[] { content }, null);
                }
            }
        }

        private void InstallServerVars()
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

                if (!serverVars.ContainsKey("umbracoPlugins"))
                    throw new Exception("Missing umbracoPlugins.");
                var umbracoPlugins = serverVars["umbracoPlugins"] as Dictionary<string, object>;
                if (umbracoPlugins == null)
                    throw new Exception("Invalid umbracoPlugins");

                umbracoPlugins["umbElasticsearch"] = SearchSettings<TSearchSettings>.Current;
            };
        }

        #endregion
    }
}