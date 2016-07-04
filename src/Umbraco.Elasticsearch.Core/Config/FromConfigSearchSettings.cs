using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Umbraco.Elasticsearch.Core.Utils;

namespace Umbraco.Elasticsearch.Core.Config
{
    public class FromConfigSearchSettings : ISearchSettings
    {
        private const string Prefix = "umbElasticsearch:";

        public string Host { get; } = nameof(Host).FromAppSettingsWithPrefix(Prefix, "http://localhost:9200");

        public string IndexEnvironmentPrefix
        {
            get
            {
                var value = nameof(IndexEnvironmentPrefix).FromAppSettingsWithPrefix(Prefix, string.Empty);
                return Environment.ExpandEnvironmentVariables(value).ToLowerInvariant();
            }
        }

        public string IndexName => nameof(IndexName).FromAppSettingsWithPrefix(Prefix);
        public IEnumerable<KeyValuePair<string, string>> AdditionalData { get; } = GetAdditionalData($"{Prefix}{nameof(AdditionalData)}:");

        private static IEnumerable<KeyValuePair<string, string>> GetAdditionalData(string prefix = Prefix)
        {
            var keys = ConfigurationManager.AppSettings.AllKeys.Where(x => x.StartsWith(prefix)).ToList();
            return keys.Select(appKey =>
            {
                var key = appKey.Replace(prefix, "");
                return new KeyValuePair<string, string>(key, ConfigurationManager.AppSettings.Get(appKey));
            });
        }
    }
}