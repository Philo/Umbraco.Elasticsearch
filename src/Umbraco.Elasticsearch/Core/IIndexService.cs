using Umbraco.Core.Models;

namespace Umbraco.Elasticsearch.Core
{
    public interface IIndexService<in TEntity> where TEntity : IContentBase
    {
        void Index(TEntity content);
        void Remove(TEntity content);

        bool IsExcludedFromIndex(TEntity content);

        bool ShouldIndex(TEntity content);

        void UpdateIndexTypeMapping();

        void ClearIndexType();
    }
}