using Nest;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Elasticsearch.Core.Media.Impl;
using Umbraco.Elasticsearch.Samplev75.Features.Search.Queries.Image;
using Umbraco.Web;

namespace Umbraco.Elasticsearch.Samplev75.Features.Search.Services.Image
{
    public class ImageMediaIndexService : MediaIndexService<ImageDocument>
    {
        protected override void Create(ImageDocument doc, IMedia entity)
        {
            doc.Extension = entity.GetValue<string>("umbracoExtension");
            var bytesAttempt = entity.GetValue<string>("umbracoBytes").TryConvertTo<long>();
            if (bytesAttempt.Success)
            {
                doc.Size = bytesAttempt.Result;
            }
        }

        public ImageMediaIndexService(IElasticClient client, UmbracoContext umbracoContext) : base(client, umbracoContext)
        {
        }
    }
}