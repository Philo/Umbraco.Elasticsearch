using System.Threading.Tasks;
using Nest;
using Nest.Queryify.Abstractions.Queries;
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

    internal class CountOfDocsForTypeQuery : ElasticClientQueryObject<ICountResponse>
    {
        private readonly string _type;
        public CountOfDocsForTypeQuery(string type)
        {
            _type = type;
        }

        protected override ICountResponse ExecuteCore(IElasticClient client, string index)
        {
            return client.Count(x => x.Type(_type).Index(index));
        }

        protected override Task<ICountResponse> ExecuteCoreAsync(IElasticClient client, string index)
        {
            return client.CountAsync(x => x.Type(_type).Index(index));
        }
    }
}
