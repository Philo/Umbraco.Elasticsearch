using System;
using System.Linq;
using Elasticsearch.Net;
using Nest;
using Umbraco.Core.Logging;
using Umbraco.Elasticsearch.Core.Config;
using Umbraco.Elasticsearch.Core.Impl;

namespace Umbraco.Elasticsearch.Core.EventHandlers
{
    public abstract partial class SearchApplicationEventHandler<TSearchSettings>
        where TSearchSettings : ISearchSettings
    {
        private void Initialise(TSearchSettings searchSettings)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(searchSettings.IndexName)) throw new ArgumentNullException(nameof(searchSettings.IndexName), "No indexName configured.  Ensure you have set am index name via ISearchSettings");
                var client = ConfigureElasticClient(searchSettings);
                UmbracoSearchFactory.SetDefaultClient(client);
                UmbracoSearchFactory.RegisterIndexStrategy(GetIndexCreationStrategy(client));
            }
            catch (Exception ex)
            {
                LogHelper.Error<SearchApplicationEventHandler<TSearchSettings>>("Unable to initialise elasticsearch integration", ex);
            }
        }

        protected virtual IElasticClient ConfigureElasticClient(TSearchSettings searchSettings)
        {
            var indexResolver = new DefaultIndexNameResolver();
            var indexName = indexResolver.Resolve(searchSettings, searchSettings.IndexName);
            return ConfigureElasticClient(searchSettings, indexName);
        }

        protected virtual IElasticClient ConfigureElasticClient(TSearchSettings searchSettings, string indexName)
        {
            var singleNodeConnectionPool = new SingleNodeConnectionPool(new Uri(searchSettings.Host));
            var connection = new ConnectionSettings(singleNodeConnectionPool);

            if (searchSettings.AdditionalData.FirstOrDefault(x => x.Key == UmbElasticsearchConstants.Configuration.EnableDebugMode).Value?.ToLowerInvariant() == "true")
            {
                connection.EnableDebugMode();
            }

            connection.DefaultIndex(indexName);
            return new ElasticClient(connection);
        }

        protected abstract IElasticsearchIndexCreationStrategy GetIndexCreationStrategy(IElasticClient client);
    }
}
