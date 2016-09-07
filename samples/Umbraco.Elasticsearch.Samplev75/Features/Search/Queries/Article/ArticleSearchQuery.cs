using Nest.Searchify.Queries;

namespace Umbraco.Elasticsearch.Samplev75.Features.Search.Queries.Article
{
    public class ArticleSearchQuery : SearchParametersFilteredQuery<ArticleSearchParameters, ArticleDocument, ArticleSearchResult>
    {
        public ArticleSearchQuery(ArticleSearchParameters parameters) : base(parameters)
        {
        }
    }
}