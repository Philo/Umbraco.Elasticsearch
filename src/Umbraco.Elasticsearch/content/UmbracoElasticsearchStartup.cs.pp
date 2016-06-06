using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using Nest.Indexify;
using Nest.Indexify.Contributors.Analysis.English;
using Nest.Indexify.Contributors.IndexSettings;
using Umbraco.Elasticsearch.Core.Config;
using Umbraco.Elasticsearch.Core.Content;
using Umbraco.Elasticsearch.Core.EventHandlers;
using Umbraco.Elasticsearch.Core.Media;

namespace $rootnamespace$
{
    public class UmbracoElasticsearchStartup : SearchApplicationEventHandler
    {
        protected override IElasticClient ConfigureElasticClient(FromConfigSearchSettings searchSettings)
        {
            var indexResolver = new DefaultIndexNameResolver();
            var indexName = indexResolver.Resolve(searchSettings, searchSettings.IndexName);
            var connection = new ConnectionSettings(new Uri(searchSettings.Host), indexName);
            return new ElasticClient(connection);
        }

        protected override IElasticsearchIndexCreationStrategy GetIndexCreationStrategy(IElasticClient client)
        {
            return new UmbracoElasticsearchIndexCreationStrategy(client);
        }

        protected override IEnumerable<IContentIndexService> RegisterContentIndexingServices()
        {
            return Enumerable.Empty<IContentIndexService>();
        }

        protected override IEnumerable<IMediaIndexService> RegisterMediaIndexingServices()
        {
            return Enumerable.Empty<IMediaIndexService>();
        }
    }

    internal class UmbracoElasticsearchIndexCreationStrategy : ElasticsearchIndexCreationStrategy
    {
        public UmbracoElasticsearchIndexCreationStrategy(IElasticClient client) : base(client)
        {
            AddContributor(new EnglishIndexAnalysisContributor());
            AddContributor(new IndexSettingsContributor(1, 1));
        }
    }
}
