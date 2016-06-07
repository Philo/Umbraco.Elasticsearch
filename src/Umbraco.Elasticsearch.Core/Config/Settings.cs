using Umbraco.Elasticsearch.Utils;

namespace Umbraco.Elasticsearch.Core.Config
{
    public class Settings
    {
        private static short DefaultInterval = 20;

        public static short AvailbilityRefreshIntervalMinutes
        {
            get
            {
                var value = nameof(AvailbilityRefreshIntervalMinutes).FromAppSettingsWithPrefix("umbElasticsearch:", "20");
                short interval;
                if (short.TryParse(value, out interval))
                {
                    interval = interval <= 0 ? DefaultInterval : interval;
                    return interval;
                }

                return DefaultInterval;
            }
        }
    }
}