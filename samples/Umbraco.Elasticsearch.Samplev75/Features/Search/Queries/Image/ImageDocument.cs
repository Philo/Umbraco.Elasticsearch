using Nest;

namespace Umbraco.Elasticsearch.Samplev75.Features.Search.Queries.Image
{
    [ElasticsearchType(Name = "image", IdProperty = "Id")]
    public class ImageDocument : UmbracoDocument
    {
        public string Extension { get; set; }
        public long Size { get; set; }
    }
}