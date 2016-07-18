using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Elasticsearch.Core
{
    public interface IIndexService<in TEntity> where TEntity : IContentBase
    {
        void Build(string indexName, Func<ServiceContext, IEnumerable<TEntity>> customRetrieveFunc = null);

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