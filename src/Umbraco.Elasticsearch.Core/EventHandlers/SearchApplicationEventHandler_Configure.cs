using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
        private static bool HasActiveIndex()
        {
            var response = UmbracoSearchFactory.Client.IndexExists(UmbracoSearchFactory.ActiveIndexName);
            return response.Exists;
        }

        private void Initialise(TSearchSettings searchSettings)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(searchSettings.IndexName)) throw new ArgumentNullException(nameof(searchSettings.IndexName), "No indexName configured.  Ensure you have set am index name via ISearchSettings");
                var indexNameResolver = GetIndexNameResolver(searchSettings);
                var client = ConfigureElasticClient(searchSettings, indexNameResolver);
                var indexStrategy = GetIndexCreationStrategy(client, indexNameResolver);

                UmbracoSearchFactory.SetDefaultClient(client);
                UmbracoSearchFactory.RegisterIndexStrategy(indexStrategy);

                UmbracoSearchFactory.HasActiveIndex = HasActiveIndex();
            }
            catch (Exception ex)
            {
                LogHelper.Error<SearchApplicationEventHandler<TSearchSettings>>("Unable to initialise elasticsearch integration", ex);
            }
        }

        protected virtual ISearchIndexNameResolver GetIndexNameResolver(TSearchSettings searchSettings)
        {
            return new DefaultIndexNameResolver(searchSettings);
        }

        private IElasticClient ConfigureElasticClient(TSearchSettings searchSettings, ISearchIndexNameResolver indexNameResolver)
        {
            var indexName = indexNameResolver.ResolveActiveIndexName(searchSettings.IndexName);
            var connection = ConfigureConnectionSettings(searchSettings, indexName);
            return ConfigureElasticClient(connection, searchSettings, indexName);
        }

        protected IConnectionSettingsValues ConfigureConnectionSettings(TSearchSettings searchSettings, string indexName)
        {
            var singleNodeConnectionPool = new SingleNodeConnectionPool(new Uri(searchSettings.Host));
            var connection = new ConnectionSettings(singleNodeConnectionPool);

            if (searchSettings.GetAdditionalData<bool>(UmbElasticsearchConstants.Configuration.EnableDebugMode))
            {
                connection.EnableDebugMode(apiCallDetails =>
                {
                    LogHelper.Debug<SearchApplicationEventHandler<TSearchSettings>>(apiCallDetails.DebugInformation);
                    Debug.WriteLine(apiCallDetails.DebugInformation);
                });
            }

            connection.DefaultIndex(indexName);

            ModifyConnectionSettings(connection, searchSettings, indexName);
            return connection;
        }

        protected virtual void ModifyConnectionSettings(ConnectionSettings connectionSettings, TSearchSettings searchSettings, string indexName)
        {
        }

        protected virtual IElasticClient ConfigureElasticClient(IConnectionSettingsValues connection, TSearchSettings searchSettings, string indexName)
        {
            return new ElasticClient(connection);
        }

        protected abstract IElasticsearchIndexCreationStrategy GetIndexCreationStrategy(IElasticClient client, ISearchIndexNameResolver indexNameResolver);
    }
}
