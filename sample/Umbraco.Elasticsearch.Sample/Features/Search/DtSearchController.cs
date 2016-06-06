using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nest;
using Nest.Queryify.Extensions;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries;
using Nest.Searchify.SearchResults;
using umbraco.DataLayer;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Elasticsearch.Sample.Features.Search
{
    public class ArticleSearchParameters : SearchParameters
    {
        
    }

    public class ArticleSearchResult : SearchResult<ArticleSearchParameters, ArticleDocument>
    {
        public ArticleSearchResult(ArticleSearchParameters parameters, ISearchResponse<ArticleDocument> response) : base(parameters, response)
        {
        }
    }

    public class ArticleSearchQuery : SearchParametersFilteredQuery<ArticleSearchParameters, ArticleDocument, ArticleSearchResult>
    {
        public ArticleSearchQuery(ArticleSearchParameters parameters) : base(parameters)
        {
        }
    }

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

    public class DtSearchController : RenderMvcController
    {
        [NonAction]
        public override ActionResult Index(RenderModel model)
        {
            return base.Index(model);
        }

        public ActionResult Index(ArticleSearchParameters parameters)
        {
            var response = UmbracoSearchFactory.Client.Query(new ArticleSearchQuery(parameters));
            var model = new SearchResultModel<ArticleSearchResult, ArticleSearchParameters, ArticleDocument>(CurrentPage);
            model.SetSearchResult(response);
            return CurrentTemplate(model);
        }
    }
}