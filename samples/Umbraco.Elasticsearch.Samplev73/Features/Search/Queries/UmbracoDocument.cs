using Nest;
using Umbraco.Elasticsearch.Core;

namespace Umbraco.Elasticsearch.Samplev73.Features.Search.Queries
{
    public class UmbracoDocument : IUmbracoDocument
    {
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Id { get; set; }

        [ElasticProperty(Analyzer = "indexify_english")]
        public string Title { get; set; }

        [ElasticProperty(Analyzer = "indexify_english")]
        public string Summary { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Url { get; set; }
    }
}
