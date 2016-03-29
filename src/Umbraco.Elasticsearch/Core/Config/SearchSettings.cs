using System.Configuration;

namespace Umbraco.Elasticsearch.Core.Config
{
    public static class SearchSettings
    {
        private static ISearchSettings _current;

        public static ISearchSettings Current
        {
            get
            {
                if (_current == null)
                    throw new ConfigurationErrorsException(string.Format("{0} have not been configured, please ensure Set() is called before accessing this configuration", typeof(SearchSettings)));
                return _current;
            }
        }

        public static void Set(ISearchSettings settings)
        {
            _current = settings;
        }
    }
}