using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using Nest.Indexify;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Elasticsearch.Core.Content;
using Umbraco.Elasticsearch.Core.Media;

namespace Umbraco.Elasticsearch.Core
{
    public static class UmbracoSearchFactory
    {
        private static IElasticClient _client;

        private readonly static IDictionary<IContentIndexService, Func<IContent, bool>> ContentIndexServiceRegistry = new Dictionary<IContentIndexService, Func<IContent, bool>>();
        private readonly static IDictionary<IMediaIndexService, Func<IMedia, bool>> MediaIndexServiceRegistry = new Dictionary<IMediaIndexService, Func<IMedia, bool>>();

        private static IElasticsearchIndexCreationStrategy _indexStrategy;

        public static IEnumerable<IContentIndexService> GetContentIndexServices()
        {
            return ContentIndexServiceRegistry.Keys;
        }

        public static IEnumerable<IMediaIndexService> GetMediaIndexServices()
        {
            return MediaIndexServiceRegistry.Keys;
        }

        public static void RegisterIndexStrategy(IElasticsearchIndexCreationStrategy strategy)
        {
            _indexStrategy = strategy;
            LogHelper.Info<IElasticsearchIndexCreationStrategy>($"Registered index strategy [{strategy.GetType().Name}]");
        }

        public static void RegisterContentIndexService<TIndexService>(TIndexService indexService, Func<IContent, bool> resolver) where TIndexService : IContentIndexService
        {
            if (!ContentIndexServiceRegistry.ContainsKey(indexService))
            {
                LogHelper.Info<TIndexService>(() => $"Registered content index service for [{indexService.GetType().Name}]");
                ContentIndexServiceRegistry.Add(indexService, resolver);
            }
            else
            {
                LogHelper.Warn<TIndexService>($"Registration for content index service [{indexService.GetType().Name}] already exists");
            }
        }

        public static void RegisterMediaIndexService<TIndexService>(TIndexService indexService, Func<IMedia, bool> resolver) where TIndexService : IMediaIndexService
        {
            if (!MediaIndexServiceRegistry.ContainsKey(indexService))
            {
                LogHelper.Info<TIndexService>(() => $"Registered media index service for [{indexService.GetType().Name}]");
                MediaIndexServiceRegistry.Add(indexService, resolver);
            }
            else
            {
                LogHelper.Warn<TIndexService>($"Registration for media index service [{indexService.GetType().Name}] already exists");
            }
        }

        public static IElasticsearchIndexCreationStrategy GetIndexStrategy()
        {
            return _indexStrategy;
        }

        public static IMediaIndexService GetMediaIndexService(IMedia media)
        {
            return MediaIndexServiceRegistry?.FirstOrDefault(x => x.Value(media)).Key;
        }

        public static IContentIndexService GetContentIndexService(IContent content)
        {
            return ContentIndexServiceRegistry?.FirstOrDefault(x => x.Value(content)).Key;
        }

        public static IElasticClient Client
        {
            get
            {
                if (_client == null) throw new ConfigurationErrorsException("Elasticsearch client is not available, verify configuration settings");
                return _client;
            }
        }
        
        public static void SetDefaultClient(IElasticClient client)
        {
            _client = client;
        }

        public static async Task<bool> IsActiveAsync()
        {
            var response = await _client.PingAsync();
            return response?.IsValid ?? false;
        }
    }
}
