namespace Umbraco.Elasticsearch.Core.Config
{
    public struct PluginVersionInfo
    {
        internal PluginVersionInfo(string umbracoVersion, string pluginVersion)
        {
            UmbracoVersion = umbracoVersion;
            PluginVersion = pluginVersion;
        }

        public string PluginVersion { get; }
        public string UmbracoVersion { get; }
    }
}