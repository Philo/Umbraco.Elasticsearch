using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Umbraco.Elasticsearch.Core.Utils;

namespace Umbraco.Elasticsearch.Core.Config
{
    public class FromConfigSearchSettings : ISearchSettings
    {
        private readonly static string Prefix = $"{UmbElasticsearchConstants.Configuration.Prefix}:";

        public string Host { get; } = nameof(Host).FromAppSettingsWithPrefix(Prefix, "http://localhost:9200");

        public string IndexEnvironmentPrefix { get; } = GetEnvironmentPrefix();

        private static string GetEnvironmentPrefix()
        {
            var value = nameof(IndexEnvironmentPrefix).FromAppSettingsWithPrefix(Prefix, "%COMPUTERNAME%");
            return Environment.ExpandEnvironmentVariables(value).ToLowerInvariant();
        }

        public string IndexName { get; } = nameof(IndexName).FromAppSettingsWithPrefix(Prefix, "umb-elasticsearch");

        public IEnumerable<KeyValuePair<string, string>> AdditionalData { get; } = GetAdditionalData($"{Prefix}{nameof(AdditionalData)}:");

        private static IEnumerable<KeyValuePair<string, string>> GetAdditionalData(string prefix)
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