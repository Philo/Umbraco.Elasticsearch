using System.Linq;
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

            var manager = new IndexManager();
            manager.Create();
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

        #endregion
    }
}