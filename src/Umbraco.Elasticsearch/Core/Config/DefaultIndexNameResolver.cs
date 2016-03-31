namespace Umbraco.Elasticsearch.Core.Config
{
    public class DefaultIndexNameResolver : ISearchIndexNameResolver
    {
        public string Resolve(ISearchSettings searchSettings, string indexName)
        {
            var separator = string.Empty;
            if (!string.IsNullOrWhiteSpace(searchSettings.IndexEnvironmentPrefix)) separator = "-";
            return $"{searchSettings.IndexEnvironmentPrefix}{separator}{indexName}";
        }
    }
}