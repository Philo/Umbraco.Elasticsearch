using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Nest;
using Nest.Indexify;
using Nest.Queryify.Abstractions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Elasticsearch.Core.Config;
using Umbraco.Elasticsearch.Core.Content;
using Umbraco.Elasticsearch.Core.Media;

namespace Umbraco.Elasticsearch.Core
{
    public static class UmbracoSearchFactory
    {
        private static IElasticsearchRepository _repository;
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

        public static IElasticsearchRepository Repository
        {
            get
            {
                if(_repository == null) throw new ConfigurationErrorsException("Elasticsearch repository is not available, verify configuration settings");
                return _repository;
            }
        }

        public static IElasticClient Client
        {
            get
            {
                if (_client == null) throw new ConfigurationErrorsException("Elasticsearch client is not available, verify configuration settings");
                return _client;
            }
        }

        public static void SetDefaultRepository(IElasticsearchRepository repository)
        {
            _repository = repository;
            IsSearchAvailable(true);
        }

        public static void SetDefaultClient(IElasticClient client)
        {
            _client = client;
            IsSearchAvailable(true);
        }
        
        private static DateTimeOffset _isSearchAvailableLastCheckDateTimeOffset;
        private static readonly object SearchAvailabilityLock = new object();

        private static bool? _isSearchAvailable;
        public static bool IsSearchAvailable(bool forceCheck = false)
        {
            if (_client == null) return false;

            if (ShouldRefreshSearchAvailability(forceCheck))
            {
                lock (SearchAvailabilityLock)
                {
                    try
                    {
                        _isSearchAvailableLastCheckDateTimeOffset = DateTimeOffset.UtcNow;
                        var pingResponse = _client.Ping();
                        _isSearchAvailable = pingResponse.IsValid;
                        LogHelper.Info(typeof(UmbracoSearchFactory), () => $"Checking search availability - HTTP Response Status = [{pingResponse.ConnectionStatus.HttpStatusCode}]");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(typeof(UmbracoSearchFactory), "No response or exception from search availability check", ex);
                        _isSearchAvailable = false;
                    }
                }
            }
            return _isSearchAvailable.GetValueOrDefault();
        }

        private static bool ShouldRefreshSearchAvailability(bool forceCheck)
        {
            return
                !_isSearchAvailable.HasValue
                || forceCheck
                || _isSearchAvailableLastCheckDateTimeOffset.Subtract(DateTimeOffset.UtcNow).TotalMinutes >= Settings.AvailbilityRefreshIntervalMinutes;
        }
    }
}
