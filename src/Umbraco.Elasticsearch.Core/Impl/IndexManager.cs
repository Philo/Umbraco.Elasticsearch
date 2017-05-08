using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
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
        private readonly IElasticClient client;
        private readonly IElasticsearchIndexCreationStrategy indexStrategy;

        public IndexManager() : this(UmbracoSearchFactory.Client, UmbracoSearchFactory.GetIndexStrategy()) { }

        public IndexManager(IElasticClient client, IElasticsearchIndexCreationStrategy indexStrategy)
        {
            this.client = client;
            this.indexStrategy = indexStrategy;
        }

        public async Task CreateAsync(bool activate = false)
        {
            await indexStrategy.CreateAsync();
        }
        
        public async Task DeleteIndexAsync(string indexName)
        {
            using (
                BusyStateManager.Start(
                    $"Deleting {indexName} triggered by '{UmbracoContext.Current.Security.CurrentUser.Name}'", indexName))
            {
                await client.DeleteIndexAsync(indexName);
            }
        }

        public async Task<IEnumerable<IndexStatusInfo>> IndicesInfo()
        {
            // TODO : validate usage and response here
            var indexAliasName = UmbracoSearchFactory.ActiveIndexName;
            var response = await client.IndicesStatsAsync($"{indexAliasName}-*");
            var indexInfo = response.Indices.Where(x => x.Key.StartsWith($"{indexAliasName}-")).Select(x => new IndexStatusInfo
            {
                Name = x.Key,
                DocCount = x.Value.Total.Documents.Count,
                Queries = x.Value.Total.Search.QueryTotal,
                SizeInBytes = x.Value.Total.Store.SizeInBytes,
                Status = GetStatus(x.Key)
            }).ToList();
            UmbracoSearchFactory.HasActiveIndex = indexInfo.Any(x => x.Status == IndexStatusOption.Active);
            return indexInfo;
        }

        public async Task<Version> GetElasticsearchVersion()
        {
            var info = await client.RootNodeInfoAsync();
            return Version.Parse(info.IsValid ? info.Version.Number : "0.0.0");
        }

        public async Task<JObject> GetIndexMappingInfo(string indexName)
        {
            var response = await client.GetMappingAsync(new GetMappingRequest(indexName, "*"));

            // TODO : Validate here
            var mappings = response.IsValid ? response.Mappings : new ReadOnlyDictionary<string, IReadOnlyDictionary<string, TypeMapping>>(null);
            var stream = new MemoryStream();
            client.Serializer.Serialize(mappings, stream);
            var r = new StreamReader(stream);
            return JObject.Parse(r.ReadToEnd());
        }

        private IndexStatusOption GetStatus(string indexName)
        {
            if (BusyStateManager.IsBusy && BusyStateManager.IndexName.Equals(indexName, StringComparison.OrdinalIgnoreCase)) return IndexStatusOption.Busy;
            var aliases = client.GetAliasesPointingToIndex(indexName).ToList();

            if (aliases.Any(x => x.Name.Equals(UmbracoSearchFactory.ActiveIndexName, StringComparison.OrdinalIgnoreCase)))
            {
                return IndexStatusOption.Active;
            }

            return IndexStatusOption.None;
        }

        public async Task ActivateIndexAsync(string indexName)
        {
            using (BusyStateManager.Start($"Activating {indexName} triggered by '{UmbracoContext.Current.Security.CurrentUser.Name}'", indexName))
            {
                var client = UmbracoSearchFactory.Client;
                var indexAliasName = UmbracoSearchFactory.ActiveIndexName;
                await client.AliasAsync(a => a
                    .Remove(r => r.Alias(indexAliasName).Index($"{indexAliasName}*"))
                    .Add(aa => aa.Alias(indexAliasName).Index(indexName))
                    );
                UmbracoSearchFactory.HasActiveIndex = true;
            }
        }
    }
}