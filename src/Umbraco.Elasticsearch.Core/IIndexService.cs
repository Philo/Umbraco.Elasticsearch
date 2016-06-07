using Umbraco.Core.Models;

namespace Umbraco.Elasticsearch.Core
{
    public interface IIndexService<in TEntity> where TEntity : IContentBase
    {
        void Index(TEntity content, string indexName);
        void Remove(TEntity content, string indexName);

        bool IsExcludedFromIndex(TEntity content);

        bool ShouldIndex(TEntity content);

        void UpdateIndexTypeMapping(string indexName);

        void ClearIndexType(string indexName);

        string EntityTypeName { get; }

        string DocumentTypeName { get; }

        long CountOfDocumentsForIndex(string indexName);
    }
}