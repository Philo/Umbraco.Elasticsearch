Umbraco.Elasticsearch
---------------------

Visit http://github.com/philo/umbraco.elasticsearch

Post-installation steps
-----------------------

If you dont already have one, you will need toi setup a SearchApplicationEventHandler like the one below:

    public class UmbracoElasticsearchStartup : SearchApplicationEventHandler
    {
        protected override IElasticClient ConfigureElasticClient(FromConfigSearchSettings searchSettings)
        {
            var indexResolver = new DefaultIndexNameResolver();
            var indexName = indexResolver.Resolve(searchSettings, searchSettings.IndexName);
            var connection = new ConnectionSettings(new Uri(searchSettings.Host), indexName);
            return new ElasticClient(connection);
        }

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


