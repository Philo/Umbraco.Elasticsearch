using System;

namespace Umbraco.Elasticsearch.Core.Config
{
    public class FromConfigSearchSettings : ISearchSettings
    {
        private const string Prefix = "SearchSettings:";

        public string Host => nameof(Host).FromAppSettingsWithPrefix(Prefix);

        public string IndexEnvironmentPrefix
        {
            get
            {
                var value = nameof(IndexEnvironmentPrefix).FromAppSettingsWithPrefix(Prefix);
                return Environment.ExpandEnvironmentVariables(value).ToLowerInvariant();
            }
        }

        public string IndexName => nameof(IndexName).FromAppSettingsWithPrefix(Prefix);
    }
}