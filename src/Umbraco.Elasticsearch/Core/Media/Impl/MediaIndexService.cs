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
    public abstract class MediaIndexService<TMediaDocument> : IMediaIndexService where TMediaDocument : MediaDocument, new()
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

        public void Index(IMedia media)
        {
            if (ShouldIndex(media))
            {
                var doc = CreateCore(media);
                _repository.Save(doc);
            }
        }

        public void Remove(IMedia media)
        {
                // this might be flawed if the document id isnt the node id
            if (_repository.Exists<TMediaDocument>(media.Id.ToString()))
            {
                _repository.Delete<TMediaDocument>(media.Id.ToString());
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

        public void UpdateIndexTypeMapping()
        {
            var mapping = _client.GetMapping<TMediaDocument>();
            if (mapping.Mapping == null && !mapping.Mappings.Any())
            {
                UpdateIndexTypeMappingCore(_client);
                LogHelper.Info<MediaIndexService<TMediaDocument>>(() => $"Updated media type mapping for '{typeof(TMediaDocument).Name}' using type name '{InitialiseIndexTypeName()}'");
            }
        }
        public void ClearIndexType()
        {
            _repository.Query(new DeleteAllOfDocumentTypeQuery<TMediaDocument>());
            UpdateIndexTypeMapping();
        }

        protected abstract void UpdateIndexTypeMappingCore(IElasticClient client);

        #region Move this to derived types
        private TMediaDocument CreateCore(IMedia mediaInstance)
        {
            var media = Helper.TypedMedia(mediaInstance.Id);

            var doc = new TMediaDocument();

            doc.NodeId = media.Id;
            doc.Title = media.Name;
            doc.Url = media.Url();

            Create(doc, media);

            return doc;
        }

        protected virtual void Create(TMediaDocument doc, IPublishedContent media)
        {
            
        }

        #endregion
    }
}