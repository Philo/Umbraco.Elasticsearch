namespace Umbraco.Elasticsearch.Core.Config
{
    public interface ISearchIndexNameResolver
    {
        string ResolveUniqueIndexName(string indexName);
        string ResolveActiveIndexName(string indexName);
    }
}