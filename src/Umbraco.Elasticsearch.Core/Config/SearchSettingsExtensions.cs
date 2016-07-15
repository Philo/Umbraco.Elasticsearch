using System.Linq;
using Umbraco.Core;

namespace Umbraco.Elasticsearch.Core.Config
{
    public static class SearchSettingsExtensions
    {
        public static T GetAdditionalData<T>(this ISearchSettings settings, string key, T defaultValue = default(T))
        {
            var keyPair = settings.AdditionalData.FirstOrDefault(x => x.Key.InvariantEquals(key));
            if (!string.IsNullOrWhiteSpace(keyPair.Key))
            {
                var attempt = keyPair.Value.TryConvertTo<T>();
                if(attempt.Success) return attempt.Result;
            }
            return defaultValue;
        }
    }
}