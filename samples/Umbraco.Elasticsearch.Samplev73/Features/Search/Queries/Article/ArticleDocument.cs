using Nest;

namespace Umbraco.Elasticsearch.Samplev73.Features.Search.Queries.Article
{
    [ElasticType(Name = "dtArticle", IdProperty = "Id")]
    public class ArticleDocument : UmbracoDocument
    {
        
    }
}