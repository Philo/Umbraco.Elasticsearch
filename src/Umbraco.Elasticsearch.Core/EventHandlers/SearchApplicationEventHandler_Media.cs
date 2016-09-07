using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Sync;
using Umbraco.Elasticsearch.Core.Config;
using Umbraco.Elasticsearch.Core.Media;
using Umbraco.Elasticsearch.Core.Utils;
using Umbraco.Web;
using Umbraco.Web.Cache;

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
            if (UmbracoSearchFactory.HasActiveIndex)
            {
                foreach (var item in media)
                {
                    var indexService = UmbracoSearchFactory.GetMediaIndexService(item);
                    var indexName = UmbracoSearchFactory.ActiveIndexName;
                    if (indexService != null)
                    {
                        try
                        {
                            if (indexService.IsExcludedFromIndex(item))
                            {
                                RemoveMedia(indexService, item, indexName);
                                if (item.IndexError()) throw new InvalidOperationException(item.GetIndexingStatus().Message);

                                messages?.Add(new EventMessage("Search", "Media removed from search index", EventMessageType.Success));
                                LogHelper.Debug(GetType(), () => $"Media ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}' has been removed from search index");
                            }
                            else
                            {
                                IndexMedia(indexService, item, indexName);
                                if (item.IndexError()) throw new InvalidOperationException(item.GetIndexingStatus().Message);

                                messages?.Add(new EventMessage("Search", "Media added to search index", EventMessageType.Success));
                                LogHelper.Debug(GetType(), () => $"Media ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}' has been indexed");
                            }
                        }
                        catch (Exception ex)
                        {
                            messages?.Add(new EventMessage("Search", $"Unable to index media : '{item.Name}' => {ex.Message}", EventMessageType.Error));
                            LogHelper.WarnWithException(GetType(), $"Unable to index media ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}'", ex);
                        }
                    }
                }
            }
            else
            {
                messages?.Add(new EventMessage("Search", "No active index available for indexing", EventMessageType.Error));
                LogHelper.Warn(GetType(), "No active index available for indexing");
            }

        }

        private void RemoveMediaCore(IEnumerable<IMedia> media, EventMessages messages)
        {
            if (UmbracoSearchFactory.HasActiveIndex)
            {
                foreach (var item in media)
                {
                    try
                    {
                        var indexService = UmbracoSearchFactory.GetMediaIndexService(item);
                        var indexName = UmbracoSearchFactory.ActiveIndexName;
                        if (indexService != null && indexService.ShouldIndex(item))
                        {
                            RemoveMedia(indexService, item, indexName);
                            if (item.IndexError()) throw new InvalidOperationException(item.GetIndexingStatus().Message);

                            messages?.Add(new EventMessage("Search", "Removed media from search index", EventMessageType.Success));
                            LogHelper.Debug(GetType(), () => $"Media ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}' has been removed from search index");
                        }
                    }
                    catch (Exception ex)
                    {
                        messages?.Add(new EventMessage("Search", $"Unable to index media : '{item.Name}' => {ex.Message}", EventMessageType.Error));
                        LogHelper.WarnWithException(GetType(), $"Unable to index media ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}'", ex);
                    }
                }
            }
            else
            {
                messages?.Add(new EventMessage("Search", "No active index available for indexing", EventMessageType.Error));
                LogHelper.Warn(GetType(), "No active index available for indexing");
            }

        }

        [Obsolete("This shouldnt be needed anymore?", true)]
        private void CacheRefresherBaseOnCacheUpdated(MediaCacheRefresher sender, CacheRefresherEventArgs cacheRefresherEventArgs)
        {
            var helper = new UmbracoHelper(UmbracoContext.Current);
            var mediaService = helper.UmbracoContext.Application.Services.MediaService;

            IMedia media = null;
            switch (cacheRefresherEventArgs.MessageType)
            {
                case MessageType.RefreshById:
                    media = mediaService.GetById((int)cacheRefresherEventArgs.MessageObject);
                    break;
                case MessageType.RefreshByInstance:
                    media = cacheRefresherEventArgs.MessageObject as IMedia;
                    break;
            }

            if (media != null)
            {
                if (media.CreateDate == media.UpdateDate)
                {
                    IndexMediaCore(new[] { media }, null);
                }
            }
        }
    }
}