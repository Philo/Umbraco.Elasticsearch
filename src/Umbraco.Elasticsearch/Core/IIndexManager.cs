namespace Umbraco.Elasticsearch.Core
{
    public interface IIndexManager
    {
        void Delete();
        void Create(bool deleteExisting = false);
    }
}