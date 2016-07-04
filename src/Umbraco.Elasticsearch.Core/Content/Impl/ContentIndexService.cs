using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using umbraco.providers.members;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Elasticsearch.Core.Impl;
using Umbraco.Web;

namespace Umbraco.Elasticsearch.Core.Content.Impl
{
    public abstract class ContentIndexService<TUmbracoDocument> : IndexService<TUmbracoDocument, IContent>, IContentIndexService
        where TUmbracoDocument : class, IUmbracoDocument, new()
    {
        protected ContentIndexService(IElasticClient client, UmbracoContext umbracoContext) : base(client, umbracoContext) { }

        protected override sealed IEnumerable<IContent> RetrieveIndexItems(ServiceContext serviceContext)
        {
            var contentType = serviceContext.ContentTypeService.GetContentType(DocumentTypeName);
            return serviceContext.ContentService.GetContentOfContentType(contentType.Id).Where(x => x.Published);
        }

        public override sealed bool ShouldIndex(IContent entity)
        {
            return entity.ContentType.Alias.Equals(IndexTypeName, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}