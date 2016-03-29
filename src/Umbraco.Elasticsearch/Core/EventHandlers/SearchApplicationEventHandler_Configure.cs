using System.Collections.Generic;
using System.Linq;
using Nest;
using Nest.Indexify;
using Nest.Queryify;
using Nest.Queryify.Abstractions;
using Umbraco.Elasticsearch.Core.Media;

namespace Umbraco.Elasticsearch.Core.EventHandlers
{
    public abstract partial class SearchApplicationEventHandler
    {

        protected virtual IEnumerable<IMediaIndexService> RegisterMediaIndexingServices()
        {
            return Enumerable.Empty<IMediaIndexService>();
        }

        public void Initialise()
        {
            var client = ConfigureElasticClient();
            var repository = ConfigureElasticsearchRepository(client);

            UmbracoSearchFactory.SetDefaultClient(client);
            UmbracoSearchFactory.SetDefaultRepository(repository);
            UmbracoSearchFactory.RegisterIndexStrategy(GetIndexCreationStrategy());
        }

        protected abstract IElasticClient ConfigureElasticClient();

        protected virtual IElasticsearchRepository ConfigureElasticsearchRepository(IElasticClient client)
        {
            return new ElasticsearchRepository(client);
        }

        protected abstract IElasticsearchIndexCreationStrategy GetIndexCreationStrategy();
    }
}
