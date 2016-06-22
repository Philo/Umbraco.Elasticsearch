using System.Globalization;
using Nest.Searchify.Abstractions;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace Umbraco.Elasticsearch.Sample.Features.Search.Models
{
    public class SearchResultModel<TSearchResult, TParameters, TDocument> : RenderModel where TSearchResult : ISearchResult<TParameters, TDocument>
        where TParameters : class, IPagingParameters, ISortingParameters, new() where TDocument : class
    {
        public SearchResultModel(IPublishedContent content, CultureInfo culture) : base(content, culture)
        {
        }

        public SearchResultModel(IPublishedContent content) : base(content)
        {
        }

        public void SetSearchResult(TSearchResult result)
        {
            Results = result;
        }

        public TSearchResult Results { get; private set; }
    }
}