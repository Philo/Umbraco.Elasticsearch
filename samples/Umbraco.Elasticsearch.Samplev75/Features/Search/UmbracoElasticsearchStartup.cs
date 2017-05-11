using System.Collections.Generic;
using System.Web;
using Nest;
using Umbraco.Elasticsearch.Core;
using Umbraco.Elasticsearch.Core.Config;
using Umbraco.Elasticsearch.Core.Content;
using Umbraco.Elasticsearch.Core.EventHandlers;
using Umbraco.Elasticsearch.Core.Impl;
using Umbraco.Elasticsearch.Core.Media;
using Umbraco.Elasticsearch.Samplev75.Features.Search.Services.Article;
using Umbraco.Elasticsearch.Samplev75.Features.Search.Services.Image;
using Umbraco.Web;

namespace Umbraco.Elasticsearch.Samplev75.Features.Search
{
    public class UmbracoElasticsearchStartup : SearchApplicationEventHandler
    {
        public UmbracoElasticsearchStartup()
        {
            SearchifyMvcConfig.Configure(HttpContext.Current.ApplicationInstance);
        }
        
        protected override IElasticsearchIndexCreationStrategy GetIndexCreationStrategy(IElasticClient client, ISearchIndexNameResolver indexNameResolver)
        {
            return new UmbracoElasticsearchIndexCreationStrategy(client, indexNameResolver);
        }

        protected override IEnumerable<IContentIndexService> RegisterContentIndexingServices()
        {
            yield return new ArticleContentIndexService(UmbracoSearchFactory.Client, UmbracoContext.Current);
        }

        protected override IEnumerable<IMediaIndexService> RegisterMediaIndexingServices()
        {
            yield return new ImageMediaIndexService(UmbracoSearchFactory.Client, UmbracoContext.Current);
        }

        internal class UmbracoElasticsearchIndexCreationStrategy : ElasticsearchIndexCreationStrategy
        {
            public UmbracoElasticsearchIndexCreationStrategy(IElasticClient client, ISearchIndexNameResolver indexNameResolver) : base(client, indexNameResolver)
            {
            }
        }
    }

}