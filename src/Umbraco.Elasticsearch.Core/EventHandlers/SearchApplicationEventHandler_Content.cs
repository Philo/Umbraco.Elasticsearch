using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Elasticsearch.Core.Config;
using Umbraco.Elasticsearch.Core.Content;

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

        private void IndexContentCore(IEnumerable<IContent> entities, EventMessages messages)
        {
            LogHelper.Debug(GetType(), () => "Content indexing triggered");
            foreach (var item in entities)
            {
                var indexService = UmbracoSearchFactory.GetContentIndexService(item);
                var indexName = UmbracoSearchFactory.Client.Infer.DefaultIndex;
                if (indexService != null)
                {
                    try
                    {
                        if (indexService.IsExcludedFromIndex(item))
                        {
                            RemoveContent(indexService, item, indexName);
                            messages?.Add(new EventMessage("Search", "Content removed from search index", EventMessageType.Success));
                            LogHelper.Debug(GetType(), () => $"Content ({item.ContentType.Alias}) '{item.Name}' with Id '{item.Id}' has been removed from search index");
                        }
                        else
                        {
                            IndexContent(indexService, item, indexName);
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

        private void RemoveContentCore(IEnumerable<IContent> entities, EventMessages messages)
        {
            foreach (var item in entities)
            {
                try
                {
                    var indexService = UmbracoSearchFactory.GetContentIndexService(item);
                    var indexName = UmbracoSearchFactory.Client.Infer.DefaultIndex;
                    if (indexService != null && indexService.ShouldIndex(item))
                    {
                        RemoveContent(indexService, item, indexName);
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
    }
}