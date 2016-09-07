using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Sync;
using Umbraco.Elasticsearch.Core.Config;
using Umbraco.Elasticsearch.Core.Content;
using Umbraco.Elasticsearch.Core.Utils;
using Umbraco.Web;
using Umbraco.Web.Cache;

namespace Umbraco.Elasticsearch.Core.EventHandlers
{
    public abstract partial class SearchApplicationEventHandler<TSearchSettings>
        where TSearchSettings : ISearchSettings
    {
        protected virtual IEnumerable<IContentIndexService> RegisterContentIndexingServices()
        {
            return Enumerable.Empty<IContentIndexService>();
        }

        /// <summary>
        /// Override me to alter the indexing process for invidual content items
        /// </summary>
        /// <param name="indexService">the index service applicable to the <param name="item">content</param></param>
        /// <param name="item">content item</param>
        /// <param name="indexName">index to add the item to</param>
        protected virtual void IndexContent(IContentIndexService indexService, IContent item, string indexName)
        {
            indexService.Index(item, indexName);
        }

        /// <summary>
        /// Override me to alter the indexing process for invidual content items
        /// </summary>
        /// <param name="indexService">the index service applicable to the <param name="item">content</param></param>
        /// <param name="item">content item</param>
        /// <param name="indexName">index to add the item to</param>
        protected virtual void RemoveContent(IContentIndexService indexService, IContent item, string indexName)
        {
            indexService.Remove(item, indexName);
        }

        private void IndexContentCore(IEnumerable<IContent> entities, EventMessages messages, [CallerMemberName] string eventCaller = null)
        {
            if (UmbracoSearchFactory.HasActiveIndex)
            {
                LogHelper.Debug(GetType(), () => $"Content indexing triggered ({eventCaller})");
                foreach (var item in entities)
                {
                    item.SetIndexingStatus(IndexingStatusOption.InProgress, "Content indexing triggered");
                    var indexService = UmbracoSearchFactory.GetContentIndexService(item);
                    var indexName = UmbracoSearchFactory.ActiveIndexName;
                    if (indexService != null)
                    {
                        try
                        {
                            if (indexService.IsExcludedFromIndex(item))
                            {
                                RemoveContent(indexService, item, indexName);
                                if (item.IndexError()) throw new InvalidOperationException(item.GetIndexingStatus().Message);

                                messages?.Add(new EventMessage("Search", "Content removed from search index", EventMessageType.Success));
                                LogHelper.Debug(GetType(), () => $"Content ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}' has been removed from search index");
                            }
                            else
                            {
                                IndexContent(indexService, item, indexName);
                                if (item.IndexError()) throw new InvalidOperationException(item.GetIndexingStatus().Message);

                                messages?.Add(new EventMessage("Search", "Content added to search index", EventMessageType.Success));
                                LogHelper.Debug(GetType(), () => $"Content ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}' has been indexed");
                            }
                        }
                        catch (Exception ex)
                        {
                            messages?.Add(new EventMessage("Search", $"Unable to index content : '{item.Name}' => {ex.Message}", EventMessageType.Error));
                            LogHelper.WarnWithException(GetType(), $"Unable to index content ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}'", ex);
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

        private void RemoveContentCore(IEnumerable<IContent> entities, EventMessages messages)
        {
            if (UmbracoSearchFactory.HasActiveIndex)
            {
                foreach (var item in entities)
                {
                    try
                    {
                        var indexService = UmbracoSearchFactory.GetContentIndexService(item);
                        var indexName = UmbracoSearchFactory.ActiveIndexName;
                        if (indexService != null && indexService.ShouldIndex(item))
                        {
                            RemoveContent(indexService, item, indexName);
                            if (item.IndexError()) throw new InvalidOperationException(item.GetIndexingStatus().Message);

                            messages?.Add(new EventMessage("Search", "Removed content from search index", EventMessageType.Success));
                            LogHelper.Debug(GetType(), () => $"Content ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}' has been removed from search index");
                        }
                    }
                    catch (Exception ex)
                    {
                        messages?.Add(new EventMessage("Search", $"Unable to remove content : '{item.Name}' => {ex.Message}", EventMessageType.Error));
                        LogHelper.WarnWithException(GetType(), $"Unable to remove content ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}' from search index", ex);
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
    }
}