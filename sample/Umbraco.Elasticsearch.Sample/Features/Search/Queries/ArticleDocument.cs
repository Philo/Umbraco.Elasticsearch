using Nest;
using Umbraco.Elasticsearch.Core;

namespace Umbraco.Elasticsearch.Sample.Features.Search
{
    [ElasticType(Name = "dtArticle", IdProperty = "Id")]
    public class ArticleDocument : UmbracoDocument
    {
        
    }
}