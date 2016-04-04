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

        public void Index(IContent content)
        {
            if (content.Published)
            {
                var doc = CreateCore(content);
                IndexCore(_repository, doc);
            }
            else
            {
                Remove(content);
            }
        }

        protected virtual void IndexCore(IElasticsearchRepository repository, TUmbracoDocument document)
        {
            repository.Save(document);
        }

        public void Remove(IContent content)
        {
            RemoveCore(_repository, content);
        }

        protected virtual void RemoveCore(IElasticsearchRepository repository, IContent content)
        {
            // this might be flawed if the document id isnt the node id
            if (repository.Exists<TUmbracoDocument>(content.Id.ToString()))
            {
                repository.Delete<TUmbracoDocument>(content.Id.ToString());
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

        public void UpdateIndexTypeMapping()
        {
            var mapping = _client.GetMapping<TUmbracoDocument>();
            if (mapping.Mapping == null && !mapping.Mappings.Any())
            {
                UpdateIndexTypeMappingCore(_client);
                LogHelper.Info<ContentIndexService<TUmbracoDocument>>(() => $"Updated content type mapping for '{typeof(TUmbracoDocument).Name}' using type name '{InitialiseIndexTypeName()}'");
            }
        }

        public void ClearIndexType()
        {
            _repository.Query(new DeleteAllOfDocumentTypeQuery<TUmbracoDocument>());
            UpdateIndexTypeMapping();
        }

        protected virtual void Create(TUmbracoDocument doc, IContent content)
        {

        }

        protected abstract void UpdateIndexTypeMappingCore(IElasticClient client);

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