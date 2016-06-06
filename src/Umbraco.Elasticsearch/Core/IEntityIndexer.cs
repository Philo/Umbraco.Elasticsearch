namespace Umbraco.Elasticsearch.Core
{
    public interface IEntityIndexer
    {
        void Build(string indexName);
    }
}