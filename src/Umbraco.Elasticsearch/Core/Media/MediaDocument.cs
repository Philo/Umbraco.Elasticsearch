using Nest;
using Umbraco.Elasticsearch.Core.Content;

namespace Umbraco.Elasticsearch.Core.Media
{
    [ElasticType(IdProperty = "NodeId", Name = "media")]
    public class MediaDocument : UmbracoDocument
    {
    }
}