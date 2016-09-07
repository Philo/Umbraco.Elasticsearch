using System.Diagnostics;
using Elasticsearch.Net;
using Nest;

namespace Umbraco.Elasticsearch.Core.Impl
{
    internal static class ElasticsearchResponseExtensions
    {
        [DebuggerStepThrough]
        internal static void EnsureException(this IResponse response)
        {
            if (response != null)
            {
                if (!response.IsValid) throw new ElasticsearchServerException(response.ServerError);
            }
        }
    }
}