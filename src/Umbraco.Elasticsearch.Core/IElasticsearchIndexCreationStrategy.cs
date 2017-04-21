using System.Threading.Tasks;

namespace Umbraco.Elasticsearch.Core
{
    public interface IElasticsearchIndexCreationStrategy
    {
        void Create();
        Task CreateAsync();
    }
}
