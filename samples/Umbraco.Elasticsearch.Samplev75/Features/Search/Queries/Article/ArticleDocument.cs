using Nest;

namespace Umbraco.Elasticsearch.Samplev75.Features.Search.Queries.Article
{
    [ElasticsearchType(Name = "dtArticle", IdProperty = "Id")]
    public class ArticleDocument : UmbracoDocument
    {
        
    }
}