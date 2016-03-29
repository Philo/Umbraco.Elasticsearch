using Umbraco.Elasticsearch.Core.Config;

namespace Umbraco.Elasticsearch
{
    public class Settings
    {
        public static short AvailbilityRefreshIntervalMinutes
        {
            get
            {
                var value = "AvailabilityRefreshIntervalMinutes".FromAppSettingsWithPrefix("Elastic", "20");
                short interval;
                if (!short.TryParse(value, out interval))
                {
                    interval = 20;
                }

                return interval;
            }
        }
    }
}