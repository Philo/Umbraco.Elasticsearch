namespace Umbraco.Elasticsearch.Core.Config
{
    public static class UmbElasticsearchConstants
    {
        public static class Configuration
        {
            public const string Prefix = "umbElasticsearch";
            public const string IndexBatchSize = nameof(IndexBatchSize);
            public const string ExcludeFromIndexPropertyAlias = nameof(ExcludeFromIndexPropertyAlias);
            public const string DisableContentCacheUpdatedEventHook = nameof(DisableContentCacheUpdatedEventHook);
        }

        public static class Properties
        {
            public const string ExcludeFromIndexAlias = "umbElasticsearchExcludeFromIndex";
        }
    }
}
