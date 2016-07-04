using Nest;

namespace Umbraco.Elasticsearch.Core
{
    public interface IUmbracoDocument
    {
        string Id { get; set; }
        //string Title { get; set; }

        //string Summary { get; set; }

        string Url { get; set; }
    }
}