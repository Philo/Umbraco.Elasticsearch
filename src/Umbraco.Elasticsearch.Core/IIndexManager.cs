using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using Newtonsoft.Json.Linq;
using Umbraco.Elasticsearch.Core.Impl;

namespace Umbraco.Elasticsearch.Core
{
    public interface IIndexManager
    {
        Task CreateAsync(bool activate = false);
        Task ActivateIndexAsync(string indexName);
        Task DeleteIndexAsync(string indexName);

        Task<IEnumerable<IndexStatusInfo>> IndicesInfo();

        Task<Version> GetElasticsearchVersion();

        Task<JObject> GetIndexMappingInfo(string indexName);
    }
}