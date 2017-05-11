using Nest;
using Umbraco.Elasticsearch.Core;

namespace Umbraco.Elasticsearch.Samplev75.Features.Search.Queries
{
    public class UmbracoDocument : IUmbracoDocument
    {
        [Keyword]
        public string Id { get; set; }

        // [ElasticProperty(Analyzer = "indexify_english")]
        [Text]
        public string Title { get; set; }

        // [ElasticProperty(Analyzer = "indexify_english")]
        [Text]
        public string Summary { get; set; }

        [Keyword]
        public string Url { get; set; }
    }
}
