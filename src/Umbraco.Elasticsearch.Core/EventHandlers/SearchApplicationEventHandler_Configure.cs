using System;
using Nest;
using Nest.Indexify;
using Umbraco.Core.Logging;
using Umbraco.Elasticsearch.Core.Config;

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
            var connection = new ConnectionSettings(new Uri(searchSettings.Host), indexName);
            return new ElasticClient(connection);
        }

        protected abstract IElasticsearchIndexCreationStrategy GetIndexCreationStrategy(IElasticClient client);
    }
}
