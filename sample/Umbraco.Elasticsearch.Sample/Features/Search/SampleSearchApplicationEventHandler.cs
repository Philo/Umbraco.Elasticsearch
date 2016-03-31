using System;
using Nest;
using Nest.Indexify;
using Nest.Indexify.Contributors.Analysis.English;
using Nest.Indexify.Contributors.IndexSettings;
using Umbraco.Elasticsearch.Core.Config;
using Umbraco.Elasticsearch.Core.EventHandlers;

namespace Umbraco.Elasticsearch.Sample.Features.Search
{
    public class UmbracoStartup : SearchApplicationEventHandler
    {
        protected override IElasticClient ConfigureElasticClient(FromConfigSearchSettings searchSettings)
        {
            var indexResolver  = new DefaultIndexNameResolver();
            var indexName = indexResolver.Resolve(searchSettings, searchSettings.IndexName);
            var connection = new ConnectionSettings(new Uri(searchSettings.Host), indexName);
            return new ElasticClient(connection);
        }

        protected override IElasticsearchIndexCreationStrategy GetIndexCreationStrategy(IElasticClient client)
        {
            return new IndexCreationStrategy(client);
        }

        private class IndexCreationStrategy : ElasticsearchIndexCreationStrategy
        {
            public IndexCreationStrategy(IElasticClient client) : base(client)
            {
                AddContributor(new EnglishIndexAnalysisContributor());
                AddContributor(new IndexSettingsContributor(1, 1));
            }
        }
    }
}