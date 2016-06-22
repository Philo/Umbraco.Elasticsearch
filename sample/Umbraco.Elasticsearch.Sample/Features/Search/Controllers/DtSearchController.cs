using System.Web.Mvc;
using Nest.Queryify.Extensions;
using Umbraco.Elasticsearch.Core;
using Umbraco.Elasticsearch.Sample.Features.Search.Models;
using Umbraco.Elasticsearch.Sample.Features.Search.Queries;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Elasticsearch.Sample.Features.Search.Controllers
{
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