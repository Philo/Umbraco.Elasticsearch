using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Elasticsearch.Core.Impl;
using Umbraco.Web;

namespace Umbraco.Elasticsearch.Core.Media.Impl
{
    public abstract class MediaIndexService<TMediaDocument> : IndexService<TMediaDocument, IMedia>, IMediaIndexService where TMediaDocument : class, IUmbracoDocument, new()
    {
        protected MediaIndexService(IElasticClient client, UmbracoContext umbracoContext) : base(client, umbracoContext) { }

        protected override sealed IEnumerable<IMedia> RetrieveIndexItems(ServiceContext serviceContext)
        {
            var mediaType = serviceContext.ContentTypeService.GetMediaType(DocumentTypeName);
            return serviceContext.MediaService.GetMediaOfMediaType(mediaType.Id).Where(x => !x.Trashed);
        }

        public override sealed bool ShouldIndex(IMedia entity)
        {
            return entity.ContentType.Alias.Equals(IndexTypeName, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}