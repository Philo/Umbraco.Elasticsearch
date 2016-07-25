Umbraco.Elasticsearch
---------------------

Visit http://github.com/philo/umbraco.elasticsearch

Post-installation steps
-----------------------

You will need to configure the location of your Elasticsearch cluster as well as any other search settings, these are configured via the <appSettings> section if you are using the default search settings implementation:

	<add key="umbElasticsearch:Host" value="http://localhost:9200" />
    <add key="umbElasticsearch:IndexName" value="umb-elasticsearch" />
    <add key="umbElasticsearch:IndexEnvironmentPrefix" value="%COMPUTERNAME%" />

These are the default values used if the keys above are not specified, you should alter the IndexName and if required the host, but I would recommend leaving the IndexEnvironmentPrefix as is for local development if you are using a shared cluster.  For other deployments I recommend settings a fixed prefix for this such as "production", "live", "test" etc.

---

If you dont already have one, you will need to setup a SearchApplicationEventHandler like the one below:

    public class UmbracoElasticsearchStartup : SearchApplicationEventHandler
    {
        protected override IElasticsearchIndexCreationStrategy GetIndexCreationStrategy(IElasticClient client)
        {
            return new UmbracoElasticsearchIndexCreationStrategy(client);
        }

        protected override IEnumerable<IContentIndexService> RegisterContentIndexingServices()
        {
            return Enumerable.Empty<IContentIndexService>();
        }

        protected override IEnumerable<IMediaIndexService> RegisterMediaIndexingServices()
        {
            return Enumerable.Empty<IMediaIndexService>();
        }

		internal class UmbracoElasticsearchIndexCreationStrategy : ElasticsearchIndexCreationStrategy
		{
			public UmbracoElasticsearchIndexCreationStrategy(IElasticClient client) : base(client)
			{
				AddContributor(new EnglishIndexAnalysisContributor());
				AddContributor(new IndexSettingsContributor(1, 1));
			}
		}
    }


