using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Umbraco.Elasticsearch.Core.Media.Impl
{
    public class MediaIndexer : IEntityIndexer
    {
        private readonly IMediaService _mediaService;
        public MediaIndexer(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        public MediaIndexer() : this(UmbracoContext.Current.Application.Services.MediaService) { }

        private static IMediaIndexService IndexServiceFor(IMedia media)
        {
            return UmbracoSearchFactory.GetMediaIndexService(media);
        }

        public void Build()
        {
            foreach (var node in _mediaService.GetRootMedia())
            {
                Publish(node, true);
            }
        }

        private void Publish(IMedia mediaInstance, bool isRecursive = false)
        {
            if (mediaInstance != null)
            {
                var indexService = IndexServiceFor(mediaInstance);
                if (indexService?.IsExcludedFromIndex(mediaInstance) ?? false)
                {
                    indexService.Remove(mediaInstance);
                }
                else
                {
                    indexService?.Index(mediaInstance);
                }

                if (isRecursive && mediaInstance.Children().Any())
                {
                    foreach (var child in mediaInstance.Children())
                    {
                        Publish(child, true);
                    }
                }
            }
        }

    }
}