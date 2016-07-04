using Nest;

namespace Umbraco.Elasticsearch.Samplev74.Features.Search.Queries.Article
{
    [ElasticType(Name = "dtArticle", IdProperty = "Id")]
    public class ArticleDocument : UmbracoDocument
    {
        
    }
}