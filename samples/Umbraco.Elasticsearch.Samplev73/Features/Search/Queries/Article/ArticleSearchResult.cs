using Nest;
using Nest.Searchify.SearchResults;

namespace Umbraco.Elasticsearch.Samplev73.Features.Search.Queries.Article
{
    public class ArticleSearchResult : SearchResult<ArticleSearchParameters, ArticleDocument>
    {
        public ArticleSearchResult(ArticleSearchParameters parameters, ISearchResponse<ArticleDocument> response) : base(parameters, response)
        {
        }
    }
}