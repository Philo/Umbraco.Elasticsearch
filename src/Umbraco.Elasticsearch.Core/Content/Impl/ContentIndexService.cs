using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Elasticsearch.Core.Config;
using Umbraco.Elasticsearch.Core.Impl;
using Umbraco.Web;

namespace Umbraco.Elasticsearch.Core.Content.Impl
{
    public abstract class ContentIndexService<TUmbracoDocument> :
        ContentIndexService<TUmbracoDocument, FromConfigSearchSettings>
        where TUmbracoDocument : class, IUmbracoDocument, new()

    {
        protected ContentIndexService(IElasticClient client, UmbracoContext umbracoContext, FromConfigSearchSettings searchSettings) : base(client, umbracoContext, searchSettings)
        {
        }

        protected ContentIndexService(IElasticClient client, UmbracoContext umbracoContext) : this(client, umbracoContext, SearchSettings<FromConfigSearchSettings>.Current)
        {
        }
    }


    public abstract class ContentIndexService<TUmbracoDocument, TSearchSettings> : IndexService<TUmbracoDocument, IContent, TSearchSettings>, IContentIndexService
        where TUmbracoDocument : class, IUmbracoDocument, new()
        where TSearchSettings : ISearchSettings
    {
        protected ContentIndexService(IElasticClient client, UmbracoContext umbracoContext, TSearchSettings searchSettings) : base(client, umbracoContext, searchSettings) { }

        protected sealed override IEnumerable<IContent> RetrieveIndexItems(ServiceContext serviceContext)
        {
            var contentType = serviceContext.ContentTypeService.GetContentType(DocumentTypeName);
            return serviceContext.ContentService.GetContentOfContentType(contentType.Id).Where(x => x.Published);
        }

        public sealed override bool ShouldIndex(IContent entity)
        {
            return entity.ContentType.Alias.Equals(IndexTypeName, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}