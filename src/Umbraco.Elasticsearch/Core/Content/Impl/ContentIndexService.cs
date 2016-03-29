using System;
using System.Linq;
using System.Reflection;
using Nest;
using Nest.Queryify.Abstractions;
using umbraco;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Elasticsearch.Core.Queries;
using Umbraco.Web;

namespace Umbraco.Elasticsearch.Core.Content.Impl
{
    public abstract class ContentIndexService<TContentDocument> : IContentIndexService where TContentDocument : ContentDocument, new()
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

        public void Index(IContent content)
        {
            if (content.Published)
            {
                var doc = CreateCore(content);
                _repository.Save(doc);
            }
            else
            {
                Remove(content);
            }
        }

        public void Remove(IContent content)
        {
            // this might be flawed if the document id isnt the node id
            if (_repository.Exists<TContentDocument>(content.Id.ToString()))
            {
                _repository.Delete<TContentDocument>(content.Id.ToString());
            }
        }

        public virtual bool IsExcludedFromIndex(IContent content)
        {
            return false;
        }

        protected virtual string InitialiseIndexTypeName()
        {
            return typeof(TContentDocument).GetCustomAttribute<ElasticTypeAttribute>()?.Name;
        }

        public virtual bool ShouldIndex(IContent content)
        {
            return content.ContentType.Alias.Equals(IndexTypeName, StringComparison.CurrentCultureIgnoreCase);
        }

        public void UpdateIndexTypeMapping()
        {
            var mapping = _client.GetMapping<TContentDocument>();
            if (mapping.Mapping == null && !mapping.Mappings.Any())
            {
                UpdateIndexTypeMappingCore(_client);
                LogHelper.Info<ContentIndexService<TContentDocument>>(() => $"Updated content type mapping for '{typeof(TContentDocument).Name}' using type name '{InitialiseIndexTypeName()}'");
            }
        }

        public void ClearIndexType()
        {
            _repository.Query(new DeleteAllOfDocumentTypeQuery<TContentDocument>());
            UpdateIndexTypeMapping();
        }

        protected virtual void Create(TContentDocument doc, IContent content)
        {

        }

        protected abstract void UpdateIndexTypeMappingCore(IElasticClient client);

        private TContentDocument CreateCore(IContent contentInstance)
        {
            var doc = new TContentDocument();

            doc.NodeId = contentInstance.Id;
            doc.Title = contentInstance.Name;
            doc.Url = library.NiceUrl(contentInstance.Id);

            Create(doc, contentInstance);
            
            return doc;
        }

    }
}