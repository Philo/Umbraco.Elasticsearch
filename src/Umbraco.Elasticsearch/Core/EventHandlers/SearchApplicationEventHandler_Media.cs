using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Elasticsearch.Core.Media;

namespace Umbraco.Elasticsearch.Core.EventHandlers
{
    public abstract partial class SearchApplicationEventHandler
    {

        /// <summary>
        /// Override me to alter the indexing process for invidual content items
        /// </summary>
        /// <param name="indexService">the index service applicable to the <param name="item">media</param></param>
        /// <param name="item">media item</param>
        protected virtual void IndexMedia(IMediaIndexService indexService, IMedia item)
        {
            indexService.Index(item);
        }

        /// <summary>
        /// Override me to alter the indexing process for invidual content items
        /// </summary>
        /// <param name="indexService">the index service applicable to the <param name="item">media</param></param>
        /// <param name="item">media item</param>
        protected virtual void RemoveMedia(IMediaIndexService indexService, IMedia item)
        {
            indexService.Remove(item);
        }

        private void IndexMediaCore(IEnumerable<IMedia> media, EventMessages messages)
        {
            foreach (var item in media)
            {
                var indexService = UmbracoSearchFactory.GetMediaIndexService(item);
                if (indexService != null)
                {
                    if (indexService.IsExcludedFromIndex(item))
                    {
                        RemoveMedia(indexService, item);
                        messages?.Add(new EventMessage("Search", "Media removed from search index", EventMessageType.Success));
                        LogHelper.Debug<SearchApplicationEventHandler>(() => $"Media ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}' has been removed from search index");
                    }
                    else
                    {
                        IndexMedia(indexService, item);
                        messages?.Add(new EventMessage("Search", "Media added to search index", EventMessageType.Success));
                        LogHelper.Debug<SearchApplicationEventHandler>(() => $"Media ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}' has been indexed");
                    }
                }
            }
        }

        private void RemoveMediaCore(IEnumerable<IMedia> media, EventMessages messages)
        {
            foreach (var item in media)
            {
                var indexService = UmbracoSearchFactory.GetMediaIndexService(item);
                if (indexService != null && indexService.ShouldIndex(item))
                {
                    RemoveMedia(indexService, item);
                    messages?.Add(new EventMessage("Search", "Removed media from search index", EventMessageType.Success));
                    LogHelper.Debug<SearchApplicationEventHandler>(() => $"Media ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}' has been removed from search index");
                }
            }
        }
    }
}