using Nest.Searchify.Queries;

namespace Umbraco.Elasticsearch.Sample.Features.Search
{
    public class ArticleSearchQuery : SearchParametersFilteredQuery<ArticleSearchParameters, ArticleDocument, ArticleSearchResult>
    {
        public ArticleSearchQuery(ArticleSearchParameters parameters) : base(parameters)
        {
        }
    }
}