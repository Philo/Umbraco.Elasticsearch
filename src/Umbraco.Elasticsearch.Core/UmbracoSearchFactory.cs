using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Nest;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Elasticsearch.Core.Config;
using Umbraco.Elasticsearch.Core.Content;
using Umbraco.Elasticsearch.Core.Impl;
using Umbraco.Elasticsearch.Core.Media;
using Umbraco.Elasticsearch.Core.Utils;

namespace Umbraco.Elasticsearch.Core
{
    public static class UmbracoSearchFactory
    {
        private static IElasticClient _client;

        private static readonly IDictionary<IContentIndexService, Func<IContent, bool>> ContentIndexServiceRegistry = new Dictionary<IContentIndexService, Func<IContent, bool>>();
        private static readonly IDictionary<IMediaIndexService, Func<IMedia, bool>> MediaIndexServiceRegistry = new Dictionary<IMediaIndexService, Func<IMedia, bool>>();

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

        public static IMediaIndexService GetMediaIndexService(Func<IMediaIndexService, bool> predicate)
        {
            return MediaIndexServiceRegistry?.Keys.FirstOrDefault(predicate);
        }

        public static IMediaIndexService GetMediaIndexService(string documentTypeName)
        {
            return MediaIndexServiceRegistry?.Keys.FirstOrDefault(x => x.DocumentTypeName.Equals(documentTypeName, StringComparison.OrdinalIgnoreCase));
        }

        public static IContentIndexService GetContentIndexService(IContent content)
        {
            return ContentIndexServiceRegistry?.FirstOrDefault(x => x.Value(content)).Key;
        }

        public static IContentIndexService GetContentIndexService(Func<IContentIndexService, bool> predicate)
        {
            return ContentIndexServiceRegistry?.Keys.FirstOrDefault(predicate);
        }
        public static IContentIndexService GetContentIndexService(string documentTypeName)
        {
            return ContentIndexServiceRegistry?.Keys.FirstOrDefault(x => x.DocumentTypeName.Equals(documentTypeName, StringComparison.OrdinalIgnoreCase));
        }

        public static IElasticClient Client
        {
            get
            {
                if (_client == null) throw new InvalidOperationException("Elasticsearch client is not available, verify configuration settings");
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

        public static PluginVersionInfo GetVersion()
        {
            var pluginVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "N/A";
            var umbracoVersion = "umbracoConfigurationStatus".FromAppSettings("N/A");
            return new PluginVersionInfo(pluginVersion,umbracoVersion);
        }

        public static string ActiveIndexName => _client.ConnectionSettings.DefaultIndex;
        public static bool HasActiveIndex { get; set; } = false;

        public static async Task<Version> GetElasticsearchVersionAsync()
        {
            var result = await _client.RootNodeInfoAsync(n => n.ErrorTrace());
            return Version.Parse(result.Version.Number);
        }
    }
}
