using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Nest;
using Nest.Queryify.Abstractions;
using umbraco;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Elasticsearch.Core.Queries;
using Umbraco.Web;

namespace Umbraco.Elasticsearch.Core.Content.Impl
{
    public abstract class ContentIndexService<TUmbracoDocument> : IContentIndexService where TUmbracoDocument : UmbracoDocument, new()
    {
        private readonly IElasticClient _client;
        private readonly IElasticsearchRepository _repository;
        private readonly Lazy<string> _indexTypeName;
        protected UmbracoHelper Helper { get; }

        protected string IndexTypeName => _indexTypeName.Value;

        protected ContentIndexService(IElasticClient client, IElasticsearchRepository repository, UmbracoContext umbracoContext)
        {
            _client = client;
            _repository = repository;
            _indexTypeName = new Lazy<string>(InitialiseIndexTypeName);
            Helper = new UmbracoHelper(umbracoContext);
        }

        protected ContentIndexService() : this(UmbracoSearchFactory.Client, UmbracoSearchFactory.Repository, UmbracoContext.Current) { }

        public void Index(IContent content, string indexName = null)
        {
            if (content.Published)
            {
                var doc = CreateCore(content);
                IndexCore(_repository, doc, indexName);
            }
            else
            {
                Remove(content, indexName);
            }
        }

        protected virtual void IndexCore(IElasticsearchRepository repository, TUmbracoDocument document, string indexName = null)
        {
            Thread.Sleep(4000);
            repository.Save(document, indexName);
        }

        public void Remove(IContent content, string indexName)
        {
            RemoveCore(_repository, content, indexName);
        }

        protected virtual void RemoveCore(IElasticsearchRepository repository, IContent content, string indexName = null)
        {
            // this might be flawed if the document id isnt the node id
            if (repository.Exists<TUmbracoDocument>(content.Id.ToString(), indexName))
            {
                repository.Delete<TUmbracoDocument>(content.Id.ToString(), indexName);
            }
        }

        public virtual bool IsExcludedFromIndex(IContent content)
        {
            return false;
        }

        protected virtual string InitialiseIndexTypeName()
        {
            return typeof(TUmbracoDocument).GetCustomAttribute<ElasticTypeAttribute>()?.Name;
        }

        public virtual bool ShouldIndex(IContent content)
        {
            return content.ContentType.Alias.Equals(IndexTypeName, StringComparison.CurrentCultureIgnoreCase);
        }

        public void UpdateIndexTypeMapping(string indexName)
        {
            var mapping = _client.GetMapping<TUmbracoDocument>(m => m.Index(indexName));
            if (mapping.Mapping == null && !mapping.Mappings.Any())
            {
                UpdateIndexTypeMappingCore(_client, indexName);
                LogHelper.Info<ContentIndexService<TUmbracoDocument>>(() => $"Updated content type mapping for '{typeof(TUmbracoDocument).Name}' using type name '{InitialiseIndexTypeName()}'");
            }
        }

        public void ClearIndexType(string indexName)
        {
            _repository.Query(new DeleteAllOfDocumentTypeQuery<TUmbracoDocument>(), indexName);
            UpdateIndexTypeMapping(indexName);
        }

        public string EntityTypeName { get; } = typeof (TUmbracoDocument).Name;

        public string DocumentTypeName { get; } =
            typeof (TUmbracoDocument).GetCustomAttribute<ElasticTypeAttribute>().Name;

        protected virtual void Create(TUmbracoDocument doc, IContent content)
        {

        }

        protected abstract void UpdateIndexTypeMappingCore(IElasticClient client, string indexName);

        private TUmbracoDocument CreateCore(IContent contentInstance)
        {
            var doc = new TUmbracoDocument();

            doc.Id = IdFor(contentInstance);
            doc.Title = contentInstance.Name;
            doc.Url = library.NiceUrl(contentInstance.Id);

            Create(doc, contentInstance);
            
            return doc;
        }

        protected virtual string IdFor(IContent contentInstance)
        {
            return contentInstance.Id.ToString();
        }
    }
}