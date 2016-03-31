using System;
using Nest;
using Nest.Indexify;
using Nest.Queryify;
using Nest.Queryify.Abstractions;
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
                var client = ConfigureElasticClient(searchSettings);
                var repository = ConfigureElasticsearchRepository(client);

                UmbracoSearchFactory.SetDefaultClient(client);
                UmbracoSearchFactory.SetDefaultRepository(repository);
                UmbracoSearchFactory.RegisterIndexStrategy(GetIndexCreationStrategy(client));
            }
            catch (Exception ex)
            {
                LogHelper.Error<SearchApplicationEventHandler<TSearchSettings>>("Unable to initialise elasticsearch integration", ex);
            }
        }

        protected abstract IElasticClient ConfigureElasticClient(TSearchSettings searchSettings);

        protected virtual IElasticsearchRepository ConfigureElasticsearchRepository(IElasticClient client)
        {
            return new ElasticsearchRepository(client);
        }

        protected abstract IElasticsearchIndexCreationStrategy GetIndexCreationStrategy(IElasticClient client);
    }
}
