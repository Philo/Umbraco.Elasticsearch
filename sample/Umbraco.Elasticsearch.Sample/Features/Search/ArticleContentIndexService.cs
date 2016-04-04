using System.Linq;
using Nest;
using Nest.Searchify;
using Umbraco.Core.Models;
using Umbraco.Elasticsearch.Core;
using Umbraco.Elasticsearch.Core.Content;
using Umbraco.Elasticsearch.Core.Content.Impl;

namespace Umbraco.Elasticsearch.Sample.Features.Search
{
    [ElasticType(Name = "dtArticle", IdProperty = "NodeId")]
    public class ArticleDocument : UmbracoDocument
    {
        
    }

    public class ArticleContentIndexService : ContentIndexService<ArticleDocument>
    {
        protected override void UpdateIndexTypeMappingCore(IElasticClient client)
        {
            client.Map<ArticleDocument>(m => m.MapFromAttributes());
        }

        protected override void Create(ArticleDocument doc, IContent content)
        {
            doc.Summary = content.GetValue<string>("summary");
        }
    }
}