using System;
using System.Threading.Tasks;
using Nest;

namespace Umbraco.Elasticsearch.Core.Impl
{
    public abstract class ElasticsearchIndexCreationStrategy : IElasticsearchIndexCreationStrategy
    {
        private readonly IElasticClient client;

        protected ElasticsearchIndexCreationStrategy(IElasticClient client)
        {
            this.client = client;
        }

        protected virtual CreateIndexDescriptor WithCreateIndexDescriptor(CreateIndexDescriptor descriptor)
        {
            return descriptor;
        }

        protected CreateIndexDescriptor WithCreateIndexDescriptorCore(CreateIndexDescriptor descriptor)
        {
            return WithCreateIndexDescriptor(descriptor);
        }

        public void Create()
        {
            // just create a new index, do not activate, so no aliasing

            var newindexName = $"{UmbracoSearchFactory.Client.ConnectionSettings.DefaultIndex}-{DateTime.UtcNow:yyyyMMddHHmmss}";

            var result = client.CreateIndex(newindexName, WithCreateIndexDescriptorCore);

            if (result.IsValid)
            {
                // adds type mappings
                Parallel.ForEach(UmbracoSearchFactory.GetContentIndexServices(), c => c.UpdateIndexTypeMapping(newindexName));
                Parallel.ForEach(UmbracoSearchFactory.GetMediaIndexServices(), c => c.UpdateIndexTypeMapping(newindexName));
            }

            // exceptions!
        }

        public async Task CreateAsync()
        {
            // just create a new index, do not activate, so no aliasing

            var newindexName = $"{UmbracoSearchFactory.Client.ConnectionSettings.DefaultIndex}-{DateTime.UtcNow:yyyyMMddHHmmss}";

            var result = await client.CreateIndexAsync(newindexName, WithCreateIndexDescriptorCore).ConfigureAwait(false);

            if (result.IsValid)
            {
                // adds type mappings
                Parallel.ForEach(UmbracoSearchFactory.GetContentIndexServices(), c => c.UpdateIndexTypeMapping(newindexName));
                Parallel.ForEach(UmbracoSearchFactory.GetMediaIndexServices(), c => c.UpdateIndexTypeMapping(newindexName));
            }

            // exceptions!
        }
    }
}