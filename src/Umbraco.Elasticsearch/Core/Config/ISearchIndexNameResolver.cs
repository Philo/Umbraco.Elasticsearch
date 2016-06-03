namespace Umbraco.Elasticsearch.Core.Config
{
    public interface ISearchIndexNameResolver
    {
        string Resolve(ISearchSettings searchSettings, string indexName);
    }
}