using System;
using System.Threading.Tasks;
using Nest;
using Nest.Indexify.Contributors;
using Umbraco.Core.Logging;

namespace Umbraco.Elasticsearch.Core.Impl
{
    public class IndexManager : IIndexManager {
        public void Create(bool activate = false)
        {
            var strategy = UmbracoSearchFactory.GetIndexStrategy();
            var indexName = UmbracoSearchFactory.Client.Infer.DefaultIndex;

            strategy.Create(new AliasedIndexContributor(activate));
            LogHelper.Info<IndexManager>(() => $"Search index '{indexName}' has been created (activate: {activate})");

            Parallel.ForEach(UmbracoSearchFactory.GetContentIndexServices(), c => c.UpdateIndexTypeMapping(indexName));
            Parallel.ForEach(UmbracoSearchFactory.GetMediaIndexServices(), c => c.UpdateIndexTypeMapping(indexName));
        }
    }

    internal class AliasedIndexContributor : ElasticsearchIndexCreationContributor, IElasticsearchIndexPreCreationContributor, IElasticsearchIndexCreationSuccessContributor
    {
        private readonly bool _activate;
        private string _timestampedIndexName;

        public AliasedIndexContributor(bool activate = false)
        {
            _activate = activate;
        }

        public override void ContributeCore(CreateIndexDescriptor descriptor, IElasticClient client)
        {
        }

        public string OnPreCreate(IElasticClient client, string indexName)
        {
            _timestampedIndexName = $"{indexName}-{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
            return _timestampedIndexName;
        }

        public void OnSuccess(IElasticClient client, IIndicesOperationResponse response)
        {
            if (_activate)
            {
                var indexName = client.Infer.DefaultIndex;
                client.Alias(a => a
                    .Remove(r => r.Alias(indexName).Index($"{indexName}*"))
                    .Add(aa => aa.Alias(indexName).Index(_timestampedIndexName))
                    );
            }
        }
    }

}