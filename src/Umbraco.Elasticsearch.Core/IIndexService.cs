using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Elasticsearch.Core
{
    public interface IIndexService<in TEntity> where TEntity : IContentBase
    {
        void Build(string indexName);

        void Index(TEntity entity, string indexName);
        void Remove(TEntity entity, string indexName);

        bool IsExcludedFromIndex(TEntity entity);

        bool ShouldIndex(TEntity entity);

        void UpdateIndexTypeMapping(string indexName);

        string EntityTypeName { get; }

        string DocumentTypeName { get; }

        long CountOfDocumentsForIndex(string indexName);
    }
}