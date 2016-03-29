using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Umbraco.Elasticsearch.Core.Content.Impl
{
    public class ContentIndexer : IEntityIndexer
    {
        private readonly IContentService _contentService;
        public ContentIndexer(IContentService contentService)
        {
            _contentService = contentService;
        }

        public ContentIndexer() : this(UmbracoContext.Current.Application.Services.ContentService) { }

        private static IContentIndexService IndexServiceFor(IContent content)
        {
            return UmbracoSearchFactory.GetContentIndexService(content);
        }

        public void Build()
        {
            foreach (var node in _contentService.GetRootContent())
            {
                Publish(node, true);
            }
        }

        private void Publish(IContent contentInstance, bool isRecursive = false)
        {
            if (contentInstance != null)
            {
                var indexService = IndexServiceFor(contentInstance);
                if (indexService?.IsExcludedFromIndex(contentInstance) ?? false)
                {
                    indexService.Remove(contentInstance);
                }
                else
                {
                    indexService?.Index(contentInstance);
                }

                if (isRecursive && contentInstance.Children().Any())
                {
                    foreach (var child in contentInstance.Children())
                    {
                        Publish(child, true);
                    }
                }
            }
        }
    }
}