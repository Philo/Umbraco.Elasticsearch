namespace Umbraco.Elasticsearch.Core.Config
{
    public class DefaultIndexNameResolver : ISearchIndexNameResolver
    {
        public string Resolve(ISearchSettings searchSettings, string indexName)
        {
            return $"{searchSettings.IndexEnvironmentPrefix}-{indexName}";
        }
    }
}