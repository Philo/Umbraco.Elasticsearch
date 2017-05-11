using System;
using System.Threading.Tasks;
using Nest;
using Umbraco.Elasticsearch.Core.Config;

namespace Umbraco.Elasticsearch.Core.Impl
{
    public abstract class ElasticsearchIndexCreationStrategy : IElasticsearchIndexCreationStrategy
    {
        private readonly IElasticClient client;
        private readonly ISearchIndexNameResolver indexNameResolver;

        protected ElasticsearchIndexCreationStrategy(IElasticClient client, ISearchIndexNameResolver indexNameResolver)
        {
            this.client = client;
            this.indexNameResolver = indexNameResolver;
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
            var newindexName = indexNameResolver.ResolveUniqueIndexName(UmbracoSearchFactory.ActiveIndexName);

            var result = client.CreateIndex(newindexName, WithCreateIndexDescriptorCore);

            if (result.IsValid)
            {
                Parallel.ForEach(UmbracoSearchFactory.GetContentIndexServices(), c => c.UpdateIndexTypeMapping(newindexName));
                Parallel.ForEach(UmbracoSearchFactory.GetMediaIndexServices(), c => c.UpdateIndexTypeMapping(newindexName));
            }
        }

        public async Task CreateAsync()
        {
            var newindexName = indexNameResolver.ResolveUniqueIndexName(UmbracoSearchFactory.ActiveIndexName);

            var result = await client.CreateIndexAsync(newindexName, WithCreateIndexDescriptorCore).ConfigureAwait(false);

            if (result.IsValid)
            {
                Parallel.ForEach(UmbracoSearchFactory.GetContentIndexServices(), c => c.UpdateIndexTypeMapping(newindexName));
                Parallel.ForEach(UmbracoSearchFactory.GetMediaIndexServices(), c => c.UpdateIndexTypeMapping(newindexName));
            }
        }
    }
}