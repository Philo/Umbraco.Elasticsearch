using System.Web;
using Nest.Searchify.Mvc.Config;

namespace Umbraco.Elasticsearch.Sample
{
    public static class SearchifyMvcConfig
    {
        public static void Configure(HttpApplication application)
        {
            SearchifyMvcConfiguration.Configure(c => c.ParameterBinding(p => p.FromThisAssembly()));
        }
    }
}