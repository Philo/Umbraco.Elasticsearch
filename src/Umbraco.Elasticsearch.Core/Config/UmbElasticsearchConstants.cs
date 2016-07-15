using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Elasticsearch.Core.Config
{
    public static class UmbElasticsearchConstants
    {
        public static class Configuration
        {
            public const string Prefix = "umbElasticsearch";
            public const string IndexBatchSize = nameof(IndexBatchSize);
            public const string ExcludeFromIndexPropertyAlias = nameof(ExcludeFromIndexPropertyAlias);
        }

        public static class Properties
        {
            public const string ExcludeFromIndexAlias = "umbElasticsearchExcludeFromIndex";
        }
    }
}
