﻿using Nest;
using Umbraco.Core.Models;
using Umbraco.Elasticsearch.Core;
using Umbraco.Elasticsearch.Core.Content.Impl;

namespace Umbraco.Elasticsearch.Sample.Features.Search
{
    [ElasticType(Name = "dtArticle", IdProperty = "Id")]
    public class ArticleDocument : UmbracoDocument
    {
        
    }

    public class ArticleContentIndexService : ContentIndexService<ArticleDocument>
    {
        protected override void UpdateIndexTypeMappingCore(IElasticClient client, string indexName)
        {
            client.Map<ArticleDocument>(m => m.MapFromAttributes().Index(indexName));
        }

        protected override void Create(ArticleDocument doc, IContent content)
        {
            doc.Summary = content.GetValue<string>("summary");
        }
    }
}