using Nest.Searchify.Queries;

namespace Umbraco.Elasticsearch.Samplev75.Features.Search.Queries.Article
{
    public class ArticleSearchQuery : SearchParametersQuery<ArticleSearchParameters, ArticleDocument, ArticleSearchResult>
    {
        public ArticleSearchQuery(ArticleSearchParameters parameters) : base(parameters)
        {
        }
    }
}