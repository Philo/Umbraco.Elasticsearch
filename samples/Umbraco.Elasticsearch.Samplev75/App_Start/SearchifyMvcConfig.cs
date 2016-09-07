using System.Web;
using Nest.Searchify.Mvc.Config;
using Nest.Searchify.Queries;

namespace Umbraco.Elasticsearch.Samplev75
{
    public static class SearchifyMvcConfig
    {
        public static void Configure(HttpApplication application)
        {
            SearchifyMvcConfiguration.Configure(c => c
                .ParameterBinding(p => p
                    .FromThisAssembly()
                    .FromAssemblyContaining<SearchParameters>()
                ));
        }
    }
}