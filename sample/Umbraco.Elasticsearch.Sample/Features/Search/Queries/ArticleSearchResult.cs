using Nest;
using Nest.Searchify.SearchResults;

namespace Umbraco.Elasticsearch.Sample.Features.Search
{
    public class ArticleSearchResult : SearchResult<ArticleSearchParameters, ArticleDocument>
    {
        public ArticleSearchResult(ArticleSearchParameters parameters, ISearchResponse<ArticleDocument> response) : base(parameters, response)
        {
        }
    }
}