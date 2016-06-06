using System;
using System.Linq;
using System.Reflection;
using Nest;
using Nest.Queryify.Abstractions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Elasticsearch.Core.Queries;
using Umbraco.Web;

namespace Umbraco.Elasticsearch.Core.Media.Impl
{
    public abstract class MediaIndexService<TMediaDocument> : IMediaIndexService where TMediaDocument : UmbracoDocument, new()
    {
        private readonly IElasticClient _client;
        private readonly IElasticsearchRepository _repository;
        private readonly Lazy<string> _indexTypeName;
        protected UmbracoHelper Helper { get; }

        protected string IndexTypeName => _indexTypeName.Value;

        protected MediaIndexService(IElasticClient client, IElasticsearchRepository repository, UmbracoContext umbracoContext)
        {
            _client = client;
            _repository = repository;
            _indexTypeName = new Lazy<string>(InitialiseIndexTypeName);
            Helper = new UmbracoHelper(umbracoContext);
        }

        protected MediaIndexService() : this(UmbracoSearchFactory.Client, UmbracoSearchFactory.Repository, UmbracoContext.Current) { }

        public void Index(IMedia media, string indexName)
        {
            if (ShouldIndex(media))
            {
                var doc = CreateCore(media);
                IndexCore(_repository, doc, indexName);
            }
        }

        protected virtual void IndexCore(IElasticsearchRepository repository, TMediaDocument document, string indexName)
        {
            repository.Save(document, indexName);
        }

        public void Remove(IMedia media, string indexName)
        {
            RemoveCore(_repository, media, indexName);
        }

        protected virtual void RemoveCore(IElasticsearchRepository repository, IMedia media, string indexName)
        {
            // this might be flawed if the document id isnt the node id
            if (repository.Exists<TMediaDocument>(media.Id.ToString(), indexName))
            {
                repository.Delete<TMediaDocument>(media.Id.ToString(), indexName);
            }
        }

        public virtual bool IsExcludedFromIndex(IMedia content)
        {
            return false;
        }

        protected virtual string InitialiseIndexTypeName()
        {
            return typeof(TMediaDocument).GetCustomAttribute<ElasticTypeAttribute>()?.Name;
        }

        public virtual bool ShouldIndex(IMedia media)
        {
            return media.ContentType.Alias.Equals(IndexTypeName, StringComparison.CurrentCultureIgnoreCase);
        }

        public void UpdateIndexTypeMapping(string indexName)
        {
            var mapping = _client.GetMapping<TMediaDocument>(m => m.Index(indexName));
            if (mapping.Mapping == null && !mapping.Mappings.Any())
            {
                UpdateIndexTypeMappingCore(_client, indexName);
                LogHelper.Info<MediaIndexService<TMediaDocument>>(() => $"Updated media type mapping for '{typeof(TMediaDocument).Name}' using type name '{InitialiseIndexTypeName()}'");
            }
        }
        public void ClearIndexType(string indexName)
        {
            _repository.Query(new DeleteAllOfDocumentTypeQuery<TMediaDocument>(), indexName);
            UpdateIndexTypeMapping(indexName);
        }

        public string EntityTypeName => typeof(TMediaDocument).Name;
        public string DocumentTypeName { get; } = typeof(TMediaDocument).GetCustomAttribute<ElasticTypeAttribute>().Name;

        protected abstract void UpdateIndexTypeMappingCore(IElasticClient client, string indexName);

        #region Move this to derived types
        private TMediaDocument CreateCore(IMedia mediaInstance)
        {
            var media = Helper.TypedMedia(mediaInstance.Id);

            var doc = new TMediaDocument();

            doc.Id = IdFor(media);
            doc.Title = media.Name;
            doc.Url = media.Url();

            Create(doc, media);

            return doc;
        }

        protected virtual string IdFor(IPublishedContent media)
        {
            return media.Id.ToString();
        }

        protected virtual void Create(TMediaDocument doc, IPublishedContent media)
        {
            
        }

        #endregion
    }
}