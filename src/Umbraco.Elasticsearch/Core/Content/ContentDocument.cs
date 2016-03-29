using System.Collections.Generic;
using System.Linq;
using Nest;

namespace Umbraco.Elasticsearch.Core.Content
{
    [ElasticType(IdProperty = "NodeId", Name = "content")]
    public class ContentDocument : UmbracoDocument
    {
        [ElasticProperty(Analyzer = "keyword")]
        public IEnumerable<string> PathForDisplay { get; set; } = Enumerable.Empty<string>();
    }
}