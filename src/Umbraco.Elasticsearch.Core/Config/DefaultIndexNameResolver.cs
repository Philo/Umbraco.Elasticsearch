using System;

namespace Umbraco.Elasticsearch.Core.Config
{
    public class DefaultIndexNameResolver : ISearchIndexNameResolver
    {
        private readonly ISearchSettings searchSettings;

        public DefaultIndexNameResolver(ISearchSettings searchSettings)
        {
            this.searchSettings = searchSettings;
        }

        public string ResolveUniqueIndexName(string indexName)
        {
            return $"{indexName}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        }

        public string ResolveActiveIndexName(string indexName)
        {
            var separator = string.Empty;
            if (!string.IsNullOrWhiteSpace(searchSettings.IndexEnvironmentPrefix)) separator = "-";
            return $"{searchSettings.IndexEnvironmentPrefix}{separator}{indexName}";
        }
    }
}