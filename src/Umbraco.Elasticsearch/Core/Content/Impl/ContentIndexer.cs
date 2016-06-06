using System.Linq;
using System.Threading.Tasks;
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

        public void Build(string indexName)
        {
            Parallel.ForEach(_contentService.GetRootContent(), node => Publish(node, indexName, true));
            /*foreach (var node in _contentService.GetRootContent())
            {
                Publish(node, indexName, true);
            }*/
        }

        private void Publish(IContent contentInstance, string indexName, bool isRecursive = false)
        {
            if (contentInstance != null)
            {
                var indexService = IndexServiceFor(contentInstance);
                if (indexService?.IsExcludedFromIndex(contentInstance) ?? false)
                {
                    indexService.Remove(contentInstance, indexName);
                }
                else
                {
                    indexService?.Index(contentInstance, indexName);
                }

                if (isRecursive && contentInstance.Children().Any())
                {
                    Parallel.ForEach(contentInstance.Children(), child => Publish(child, indexName, true));
                    //foreach (var child in contentInstance.Children())
                    //{
                    //    Publish(child, indexName, true);
                    //}
                }
            }
        }
    }
}