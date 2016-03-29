using Umbraco.Core.Models;

namespace Umbraco.Elasticsearch.Core.Content
{
    public interface IContentIndexService : IContentIndexService<IContent>
    {

    }

    public interface IContentIndexService<in TContent> : IIndexService<TContent> where TContent : IContent
    {

    }

}