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
        public void Initialise(TSearchSettings searchSettings)
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

        protected abstract IElasticClient ConfigureElasticClient(TSearchSettings searchSettings);

        protected abstract IElasticsearchIndexCreationStrategy GetIndexCreationStrategy(IElasticClient client);
    }
}
