using System;
using System.Configuration;

namespace Umbraco.Elasticsearch.Core.Config
{
    public static class SearchSettings<TSearchSettings> where TSearchSettings : ISearchSettings
    {
        private static TSearchSettings _current;

        public static TSearchSettings Current
        {
            get
            {
                if (_current == null)
                    throw new InvalidOperationException(
                        "please ensure Set() is called before accessing this configuration");
                return _current;
            }
        }

        public static void Set(TSearchSettings settings)
        {
            _current = settings;
        }
    }
}