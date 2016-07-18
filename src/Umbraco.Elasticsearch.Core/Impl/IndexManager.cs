using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using Nest.Indexify;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Logging;
using Umbraco.Web;

namespace Umbraco.Elasticsearch.Core.Impl
{
    public enum IndexStatusOption
    {
        None,
        Active,
        Busy
    }

    public struct IndexStatusInfo
    {
        public string Name { get; set; }
        public long DocCount { get; set; }
        public long Queries { get; set; }
        public double SizeInBytes { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public IndexStatusOption Status { get; set; }
    }

    public class IndexManager : IIndexManager {
        private readonly IElasticClient _client;
        private readonly IElasticsearchIndexCreationStrategy _indexStrategy;

        public IndexManager() : this(UmbracoSearchFactory.Client, UmbracoSearchFactory.GetIndexStrategy()) { }

        public IndexManager(IElasticClient client, IElasticsearchIndexCreationStrategy indexStrategy)
        {
            _client = client;
            _indexStrategy = indexStrategy;
        }

        public async Task CreateAsync(bool activate = false)
        {
            var aliasContributor = new AliasedIndexContributor(activate);
            aliasContributor.OnSuccessEventHandler += 
                (sender, args) =>
                    LogHelper.Info<IndexManager>(
                        $"Search index '{args.IndexAliasedTo}' has been created (activated: {args.Activated})");

            await _indexStrategy.CreateAsync(aliasContributor);
        }
        
        public async Task DeleteIndexAsync(string indexName)
        {
            using (
                BusyStateManager.Start(
                    $"Deleting {indexName} triggered by '{UmbracoContext.Current.Security.CurrentUser.Name}'", indexName))
            {
                await _client.DeleteIndexAsync(d => d.Index(indexName));
            }
        }

        public async Task<IEnumerable<IndexStatusInfo>> IndicesInfo()
        {
            var response = await _client.IndicesStatsAsync();
            var indexAliasName = _client.Infer.DefaultIndex;
            return response.Indices.Where(x => x.Key.StartsWith($"{indexAliasName}-")).Select(x => new IndexStatusInfo
            {
                Name = x.Key,
                DocCount = x.Value.Total.Documents.Count,
                Queries = x.Value.Total.Search.QueryTotal,
                SizeInBytes = x.Value.Total.Store.SizeInBytes,
                Status = GetStatus(x.Key)
            });
        }

        public async Task<Version> GetElasticsearchVersion()
        {
            var info = await _client.RootNodeInfoAsync();
            return Version.Parse(info.IsValid ? info.Version.Number : "0.0.0");
        }

        public async Task<JObject> GetIndexMappingInfo(string indexName)
        {
            var response = await _client.GetMappingAsync(new GetMappingRequest(indexName, "*"));

            var mappings = response.IsValid ? response.Mappings : new Dictionary<string, IList<TypeMapping>>();
            var raw = _client.Serializer.Serialize(mappings);
            return JObject.Parse(Encoding.UTF8.GetString(raw));
        }

        private IndexStatusOption GetStatus(string indexName)
        {
            if (BusyStateManager.IsBusy && BusyStateManager.IndexName.Equals(indexName, StringComparison.InvariantCultureIgnoreCase)) return IndexStatusOption.Busy;
            return _client.AliasExists(x => x.Index(indexName).Name(_client.Infer.DefaultIndex)).Exists ? IndexStatusOption.Active : IndexStatusOption.None;
        }

        public async Task ActivateIndexAsync(string indexName)
        {
            using (BusyStateManager.Start($"Activating {indexName} triggered by '{UmbracoContext.Current.Security.CurrentUser.Name}'", indexName))
            {
                var client = UmbracoSearchFactory.Client;
                var indexAliasName = client.Infer.DefaultIndex;
                await client.AliasAsync(a => a
                    .Remove(r => r.Alias(indexAliasName).Index($"{indexAliasName}*"))
                    .Add(aa => aa.Alias(indexAliasName).Index(indexName))
                    );
            }
        }
    }
}