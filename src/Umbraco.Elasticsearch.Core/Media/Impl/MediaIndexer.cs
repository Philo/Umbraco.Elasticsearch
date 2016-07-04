using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Elasticsearch.Core.Content.Impl;
using Umbraco.Web;

namespace Umbraco.Elasticsearch.Core.Media.Impl
{
    /*
    public abstract class EntityIndexer : IEntityIndexer
    {
        private readonly UmbracoContext _umbracoContext;

        protected EntityIndexer(UmbracoContext umbracoContext)
        {
            _umbracoContext = umbracoContext;
        }

        private void BuildForIndexService(IMediaIndexService indexService, string indexName)
        {
            var mt = _umbracoContext.Application.Services.MediaService;
            var cts = _umbracoContext.Application.Services.ContentTypeService;
            var mediaType = cts.GetContentType(indexService.DocumentTypeName);
            var contentList = mt.GetMediaOfMediaType(mediaType.Id).Where(x => !x.Trashed).ToList();

            LogHelper.Info<MediaIndexer>($"Started building index for {mediaType.Alias} (total: {contentList.Count}) in [{indexName}]");
            indexService.Update(contentList, indexName);
        }

        protected abstract 

        public void Build(string indexName)
        {
            foreach (var mediaIndexService in UmbracoSearchFactory.GetMediaIndexServices())
            {
                BuildForIndexService(mediaIndexService, indexName);
            }
        }
    } */

    public class MediaIndexer : IEntityIndexer
    {
        private readonly UmbracoContext _umbracoContext;
        public MediaIndexer(UmbracoContext umbracoContext)
        {
            _umbracoContext = umbracoContext;
        }

        public MediaIndexer() : this(UmbracoContext.Current) { }

        private static IMediaIndexService IndexServiceFor(IMedia media)
        {
            return UmbracoSearchFactory.GetMediaIndexService(media);
        }

        public void Build(string indexName)
        {
            foreach (var indexService in UmbracoSearchFactory.GetMediaIndexServices())
            {
                indexService.Build(indexName);
            }
        }

        /*
        private void Publish(IMedia mediaInstance, string indexName, bool isRecursive = false)
        {
            if (mediaInstance != null)
            {
                var indexService = IndexServiceFor(mediaInstance);
                if (indexService?.IsExcludedFromIndex(mediaInstance) ?? false)
                {
                    indexService.Remove(mediaInstance, indexName);
                }
                else
                {
                    indexService?.Index(mediaInstance, indexName);
                }

                if (isRecursive && mediaInstance.Children().Any())
                {
                    foreach (var child in mediaInstance.Children())
                    {
                        Publish(child, indexName, true);
                    }
                }
            }
        } */

    }
}