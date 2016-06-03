using Nest;
using Nest.Searchify;

namespace Umbraco.Elasticsearch.Core
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

        public FilterField Type { get; set; } = FilterField.Empty();
    }
}
