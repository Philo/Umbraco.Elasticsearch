using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Elasticsearch.Core.Config;
using Umbraco.Elasticsearch.Core.Media;

namespace Umbraco.Elasticsearch.Core.EventHandlers
{
    public abstract partial class SearchApplicationEventHandler<TSearchSettings>
        where TSearchSettings : ISearchSettings
    {
        /// <summary>
        /// Override me to register media index services
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<IMediaIndexService> RegisterMediaIndexingServices()
        {
            return Enumerable.Empty<IMediaIndexService>();
        }

        /// <summary>
        /// Override me to alter the indexing process for individual content items
        /// </summary>
        /// <param name="indexService">the index service applicable to the <param name="item">media</param></param>
        /// <param name="item">media item</param>
        /// <param name="indexName">index to add the item to</param>
        protected virtual void IndexMedia(IMediaIndexService indexService, IMedia item, string indexName)
        {
            indexService.Index(item, indexName);
        }

        /// <summary>
        /// Override me to alter the indexing process for invidual content items
        /// </summary>
        /// <param name="indexService">the index service applicable to the <param name="item">media</param></param>
        /// <param name="item">media item</param>
        /// <param name="indexName">index to add the item to</param>
        protected virtual void RemoveMedia(IMediaIndexService indexService, IMedia item, string indexName)
        {
            indexService.Remove(item, indexName);
        }

        private void IndexMediaCore(IEnumerable<IMedia> media, EventMessages messages)
        {
            foreach (var item in media)
            {
                var indexService = UmbracoSearchFactory.GetMediaIndexService(item);
                var indexName = UmbracoSearchFactory.Client.Infer.DefaultIndex;
                if (indexService != null)
                {
                    if (indexService.IsExcludedFromIndex(item))
                    {
                        RemoveMedia(indexService, item, indexName);
                        messages?.Add(new EventMessage("Search", "Media removed from search index", EventMessageType.Success));
                        LogHelper.Debug<SearchApplicationEventHandler<TSearchSettings>>(() => $"Media ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}' has been removed from search index");
                    }
                    else
                    {
                        IndexMedia(indexService, item, indexName);
                        messages?.Add(new EventMessage("Search", "Media added to search index", EventMessageType.Success));
                        LogHelper.Debug<SearchApplicationEventHandler<TSearchSettings>>(() => $"Media ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}' has been indexed");
                    }
                }
            }
        }

        private void RemoveMediaCore(IEnumerable<IMedia> media, EventMessages messages)
        {
            foreach (var item in media)
            {
                var indexService = UmbracoSearchFactory.GetMediaIndexService(item);
                var indexName = UmbracoSearchFactory.Client.Infer.DefaultIndex;
                if (indexService != null && indexService.ShouldIndex(item))
                {
                    RemoveMedia(indexService, item, indexName);
                    messages?.Add(new EventMessage("Search", "Removed media from search index", EventMessageType.Success));
                    LogHelper.Debug<SearchApplicationEventHandler<TSearchSettings>>(() => $"Media ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}' has been removed from search index");
                }
            }
        }
    }
}