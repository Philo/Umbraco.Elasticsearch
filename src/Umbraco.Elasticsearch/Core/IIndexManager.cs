using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;

namespace Umbraco.Elasticsearch.Core
{
    public interface IIndexManager
    {
        void Create(bool activate = false);
    }
}