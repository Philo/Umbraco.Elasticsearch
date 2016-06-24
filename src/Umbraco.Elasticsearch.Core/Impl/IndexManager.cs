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
            var aliasContributor = new AliasedIndexContributor(activate);
            aliasContributor.OnSuccessEventHandler += 
                (sender, args) =>
                    LogHelper.Info<IndexManager>(
                        $"Search index '{args.IndexAliasedTo}' has been created (activated: {args.Activated})");

            strategy.Create(aliasContributor);
        }
    }

    internal class AliasedIndexContributor : ElasticsearchIndexCreationContributor, IElasticsearchIndexPreCreationContributor, IElasticsearchIndexCreationSuccessContributor
    {
        public class AliasedIndexSuccessEventArgs : EventArgs
        {
            public string IndexAliasedTo { get; }
            public bool Activated { get; }
            internal AliasedIndexSuccessEventArgs(string indexAliasedTo, bool activated)
            {
                IndexAliasedTo = indexAliasedTo;
                Activated = activated;
            }
        }

        private string _indexAliasedTo;
        private readonly bool _activate;
        public event EventHandler<AliasedIndexSuccessEventArgs> OnSuccessEventHandler;

        public AliasedIndexContributor(bool activate = false)
        {
            _activate = activate;
        }

        public override void ContributeCore(CreateIndexDescriptor descriptor, IElasticClient client)
        {
        }

        public string OnPreCreate(IElasticClient client, string indexName)
        {
            _indexAliasedTo = _indexAliasedTo ?? $"{indexName}-{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
            return _indexAliasedTo;
        }

        public void OnSuccess(IElasticClient client, IIndicesOperationResponse response)
        {
            if (_activate)
            {
                var indexName = client.Infer.DefaultIndex;
                client.Alias(a => a
                    .Remove(r => r.Alias(indexName).Index($"{indexName}*"))
                    .Add(aa => aa.Alias(indexName).Index(_indexAliasedTo))
                    );
            }
            Parallel.ForEach(UmbracoSearchFactory.GetContentIndexServices(), c => c.UpdateIndexTypeMapping(_indexAliasedTo));
            Parallel.ForEach(UmbracoSearchFactory.GetMediaIndexServices(), c => c.UpdateIndexTypeMapping(_indexAliasedTo));
            OnSuccessEventHandler?.Invoke(this, new AliasedIndexSuccessEventArgs(_indexAliasedTo, _activate));
        }
    }

}