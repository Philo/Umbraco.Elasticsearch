using Nest;
using Umbraco.Core.Models;
using Umbraco.Elasticsearch.Core.Content.Impl;
using Umbraco.Elasticsearch.Samplev75.Features.Search.Queries.Article;
using Umbraco.Web;

namespace Umbraco.Elasticsearch.Samplev75.Features.Search.Services.Article
{
    public class ArticleContentIndexService : ContentIndexService<ArticleDocument>
    {
        protected override void Create(ArticleDocument doc, IContent content)
        {
            doc.Title = content.Name;
            doc.Summary = content.GetValue<string>("summary");
        }

        public ArticleContentIndexService(IElasticClient client, UmbracoContext umbracoContext) : base(client, umbracoContext)
        {
        }
    }
}