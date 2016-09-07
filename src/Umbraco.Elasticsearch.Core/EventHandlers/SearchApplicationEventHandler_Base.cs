using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
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
        }

        protected sealed override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            DashboardHelper.EnsureSection("umbElasticsearch", "Elasticsearch",
                "/App_Plugins/umbElasticsearch/umbElasticsearch.html");

            InstallServerVars();
        }

        protected sealed override void ApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            LogHelper.Info<SearchApplicationEventHandler<TSearchSettings>>(() => "Initialising configuration for Elasticsearch integration");
            Initialise(SearchSettings<TSearchSettings>.Current);
        }

        protected sealed override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Published += ContentService_Published;
            ContentService.UnPublished += ContentServiceOnUnPublished;
            ContentService.Trashed += ContentServiceOnTrashed;
            ContentService.Moved += ContentServiceOnMoved;
            ContentService.Copied += ContentServiceOnCopied;
            ContentService.RolledBack += ContentServiceOnRolledBack;

            //CacheRefresherBase<MediaCacheRefresher>.CacheUpdated += CacheRefresherBaseOnCacheUpdated;
            //CacheRefresherBase<PageCacheRefresher>.CacheUpdated += CacheRefresherBaseOnCacheUpdated;

            MediaService.Moved += MediaServiceOnMoved;
            MediaService.Saved += MediaServiceOnSaved;
            MediaService.Deleted += MediaServiceOnDeleted;

            foreach (var service in RegisterContentIndexingServices())
            {
                UmbracoSearchFactory.RegisterContentIndexService(service, service.ShouldIndex);
            }

            foreach (var service in RegisterMediaIndexingServices())
            {
                UmbracoSearchFactory.RegisterMediaIndexService(service, service.ShouldIndex);
            }

            // check for activate index
            var m = new IndexManager();
            var activeIndex = m.IndicesInfo().Result.FirstOrDefault(x => x.Status == IndexStatusOption.Active);
            LogHelper.Info<SearchApplicationEventHandler>($"Active Index: {activeIndex.Name}");
        }

        private void ContentServiceOnRolledBack(IContentService sender, RollbackEventArgs<IContent> rollbackEventArgs)
        {
            IndexContentCore(rollbackEventArgs.Entity.AsEnumerableOfOne(), rollbackEventArgs.Messages);
        }

        private void ContentServiceOnCopied(IContentService sender, CopyEventArgs<IContent> copyEventArgs)
        {
            IndexContentCore(copyEventArgs.Copy.AsEnumerableOfOne(), copyEventArgs.Messages);
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

        private void ContentServiceOnMoved(IContentService sender, MoveEventArgs<IContent> moveEventArgs)
        {
            IndexContentCore(moveEventArgs.MoveInfoCollection.Select(x => x.Entity), moveEventArgs.Messages);
        }

        private void MediaServiceOnMoved(IMediaService sender, MoveEventArgs<IMedia> moveEventArgs)
        {
            IndexMediaCore(moveEventArgs.MoveInfoCollection.Select(x => x.Entity), moveEventArgs.Messages);
        }

        private void MediaServiceOnDeleted(IMediaService sender, DeleteEventArgs<IMedia> deleteEventArgs)
        {
            RemoveMediaCore(deleteEventArgs.DeletedEntities, deleteEventArgs.Messages);
        }

        private void MediaServiceOnSaved(IMediaService sender, SaveEventArgs<IMedia> saveEventArgs)
        {
            IndexMediaCore(saveEventArgs.SavedEntities, saveEventArgs.Messages);
        }

        private static void InstallServerVars()
        {
            ServerVariablesParser.Parsing += (sender, serverVars) =>
            {
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