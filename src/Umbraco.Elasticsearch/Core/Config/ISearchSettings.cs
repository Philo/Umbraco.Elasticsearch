namespace Umbraco.Elasticsearch.Core.Config
{
    public interface ISearchSettings
    {
        string Host { get; }
        string IndexEnvironmentPrefix { get; }
        string IndexName { get; }
    }
}