using Nest;
using Nest.Queryify.Queries;

namespace Umbraco.Elasticsearch.Core.Queries
{
    internal class DeleteAllOfDocumentTypeQuery<TDocumentType> : DeleteWithQueryObject<TDocumentType> where TDocumentType : class
    {
        protected override DeleteByQueryDescriptor<TDocumentType> BuildQuery(DeleteByQueryDescriptor<TDocumentType> descriptor)
        {
            return descriptor
                .Type<TDocumentType>()
                .MatchAll();
        }
    }
}
