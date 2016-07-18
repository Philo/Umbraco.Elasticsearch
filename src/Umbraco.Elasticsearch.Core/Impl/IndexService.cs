using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nest;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Elasticsearch.Core.Config;
using Umbraco.Elasticsearch.Core.Utils;
using Umbraco.Web;

namespace Umbraco.Elasticsearch.Core.Impl
{
    public abstract class IndexService<TUmbracoDocument, TUmbracoEntity, TSearchSettings> : IIndexService<TUmbracoEntity>
        where TUmbracoEntity : class, IContentBase 
        where TUmbracoDocument : class, IUmbracoDocument, new()
        where TSearchSettings : ISearchSettings
    {
        private readonly IElasticClient _client;
        private readonly UmbracoContext _umbracoContext;
        private readonly Lazy<string> _indexTypeName;

        protected string IndexTypeName => _indexTypeName.Value;

        [Obsolete("Bad usage, the UmbracoHelper should be created as needed and not be a long lived instance", true)]
        protected UmbracoHelper Helper { get; }

        protected TSearchSettings SearchSettings { get; }

        protected IndexService(IElasticClient client, UmbracoContext umbracoContext, TSearchSettings searchSettings)
        {
            _client = client;
            _umbracoContext = umbracoContext;
            SearchSettings = searchSettings;
            _indexTypeName = new Lazy<string>(InitialiseIndexTypeName);
        }

        protected IndexService(TSearchSettings searchSettings)
            : this(UmbracoSearchFactory.Client, UmbracoContext.Current, searchSettings)
        {
        }

        protected virtual string InitialiseIndexTypeName()
        {
            return typeof (TUmbracoDocument).GetCustomAttribute<ElasticTypeAttribute>()?.Name;
        }

        public void Index(TUmbracoEntity entity, string indexName = null)
        {
            if (!IsExcludedFromIndex(entity))
            {
                var doc = CreateCore(entity);
                IndexCore(_client, doc, indexName);
            }
            else
            {
                Remove(entity, indexName);
            }
        }

        protected virtual void IndexCore(IElasticClient client, TUmbracoDocument document,
            string indexName = null)
        {
            client.Index(document, i => i.Index(indexName).Id(document.Id));
        }

        public void UpdateIndexTypeMapping(string indexName)
        {
            var mapping = _client.GetMapping<TUmbracoDocument>(m => m.Index(indexName));
            if (mapping.Mapping == null && !mapping.Mappings.Any())
            {
                UpdateIndexTypeMappingCore(_client, indexName);
                LogHelper.Info(GetType(),
                    () =>
                        $"Updated content type mapping for '{typeof (TUmbracoDocument).Name}' using type name '{InitialiseIndexTypeName()}'");
            }
        }

        public string EntityTypeName { get; } = typeof (TUmbracoDocument).Name;

        public string DocumentTypeName { get; } =
            typeof (TUmbracoDocument).GetCustomAttribute<ElasticTypeAttribute>().Name;

        public long CountOfDocumentsForIndex(string indexName)
        {
            var response = _client.Count(c => c.Index(indexName).Type(DocumentTypeName));
            if (response.IsValid)
            {
                return response.Count;
            }
            return -1;
        }

        protected abstract IEnumerable<TUmbracoEntity> RetrieveIndexItems(ServiceContext serviceContext);

        protected virtual void RemoveFromIndex(IList<string> ids, string indexName)
        {
            if (ids.Any())
            {
                UmbracoSearchFactory.Client.Bulk(
                    b => b.DeleteMany<TUmbracoDocument>(ids, (desc, id) => desc.Index(indexName)).Refresh());
            }
        }

        protected virtual void AddOrUpdateIndex(IList<TUmbracoDocument> docs, string indexName, int pageSize = 500)
        {
            if (docs.Any())
            {
                LogHelper.Info(GetType(), () => $"Indexing {docs.Count} {DocumentTypeName} documents into {indexName}");
                var response = _client.Bulk(b => b.IndexMany(docs, (desc, doc) => desc.Index(indexName).Id(doc.Id)));
                if (response.Errors)
                {
                    LogHelper.Warn(GetType(), $"There were errors during bulk indexing, {response.ItemsWithErrors.Count()} items failed");
                }
                LogHelper.Info(GetType(), () => $"Finished indexing {docs.Count} {DocumentTypeName} documents into {indexName}");
            }

        }

        public void Build(string indexName, Func<ServiceContext, IEnumerable<TUmbracoEntity>> customRetrieveFunc = null)
        {
            var pageSize = IndexBatchSize;
            LogHelper.Info(GetType(), () => $"Starting to index [{DocumentTypeName}] into {indexName} (custom retrieval: {customRetrieveFunc != null})");
            var retrievedItems = customRetrieveFunc?.Invoke(_umbracoContext.Application.Services) ?? RetrieveIndexItems(_umbracoContext.Application.Services);
            
            foreach (var contentList in retrievedItems.Page(pageSize))
            {
                var contentGroups = contentList.ToLookup(IsExcludedFromIndex, c => c);
                RemoveFromIndex(contentGroups[true].Select(x => x.Id.ToString()).ToList(), indexName);
                AddOrUpdateIndex(contentGroups[false].Select(CreateCore).Where(x => x != null).ToList(), indexName, pageSize);
            }
            _client.Refresh(i => i.Index(indexName));

            LogHelper.Info(GetType(), () => $"Finished indexing [{DocumentTypeName}] into {indexName}");
        }

        protected virtual int IndexBatchSize => SearchSettings.GetAdditionalData(UmbElasticsearchConstants.Configuration.IndexBatchSize, 500);

        protected abstract void Create(TUmbracoDocument doc, TUmbracoEntity entity);

        protected virtual void UpdateIndexTypeMappingCore(IElasticClient client, string indexName)
        {
            client.Map<TUmbracoDocument>(m => m.MapFromAttributes().Index(indexName));
        }

        private TUmbracoDocument CreateCore(TUmbracoEntity contentInstance)
        {
            try
            {
                var doc = new TUmbracoDocument
                {
                    Id = IdFor(contentInstance),
                    Url = UrlFor(contentInstance)
                };

                Create(doc, contentInstance);

                return doc;
            }
            catch (Exception ex)
            {
                LogHelper.Error(GetType(), $"Unable to create {DocumentTypeName} due to an exception", ex);
                return null;
            }
        }

        public void Remove(TUmbracoEntity entity, string indexName)
        {
            RemoveCore(_client, entity, indexName);
        }

        protected virtual void RemoveCore(IElasticClient client, TUmbracoEntity entity,
            string indexName = null)
        {
            var idValue = IdFor(entity);
            if (client.DocumentExists<TUmbracoDocument>(d => d.Id(idValue).Index(indexName)).Exists)
            {
                client.Delete<TUmbracoDocument>(d => d.Index(indexName).Id(idValue));
            }
        }

        public virtual bool IsExcludedFromIndex(TUmbracoEntity entity)
        {
            var propertyAlias = SearchSettings.GetAdditionalData(UmbElasticsearchConstants.Configuration.ExcludeFromIndexPropertyAlias, UmbElasticsearchConstants.Properties.ExcludeFromIndexAlias);
            return entity.HasProperty(propertyAlias) && entity.GetValue<bool>(propertyAlias);
        }

        public abstract bool ShouldIndex(TUmbracoEntity entity);

        protected virtual string UrlFor(TUmbracoEntity contentInstance)
        {
            return UmbracoContext.Current.UrlProvider.GetUrl(contentInstance.Id);
        }

        protected virtual string IdFor(TUmbracoEntity contentInstance)
        {
            return contentInstance.Id.ToString();
        }
    }
}