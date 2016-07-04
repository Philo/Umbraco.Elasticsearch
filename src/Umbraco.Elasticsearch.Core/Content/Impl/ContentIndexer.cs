using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Umbraco.Elasticsearch.Core.Content.Impl
{
    public class ContentIndexer : IEntityIndexer
    {
        private readonly UmbracoContext _umbracoContext;
        private readonly Stopwatch _stopWatch;

        public ContentIndexer(UmbracoContext umbracoContext)
        {
            _umbracoContext = umbracoContext;
            _stopWatch = new Stopwatch();
        }

        public ContentIndexer() : this(UmbracoContext.Current) { }
        
        public void Build(string indexName)
        {
            _stopWatch.Restart();
            LogHelper.Info<ContentIndexer>($"Started building index [{indexName}]");
            foreach (var indexService in UmbracoSearchFactory.GetContentIndexServices())
            {
                try
                {
                    LogHelper.Info<ContentIndexer>($"Started to index content for {indexService.DocumentTypeName}");
                    indexService.Build(indexName);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<ContentIndexer>($"Failed to index content for {indexService.DocumentTypeName}", ex);
                }
            }
            _stopWatch.Stop();
            LogHelper.Info<ContentIndexer>($"Finished building index [{indexName}] : elapsed {_stopWatch.Elapsed.ToString("g")}");
        }

        /*
        private void Publish(IContent contentInstance, string indexName, bool isRecursive = false)
        {
            if (contentInstance != null)
            {
                var indexService = IndexServiceFor(contentInstance);
                if (indexService?.IsExcludedFromIndex(contentInstance) ?? false)
                {
                    LogHelper.Info<ContentIndexer>($"- [{indexName}] Removing {NodeDisplayPath(contentInstance.Path)} : elapsed {_stopWatch.Elapsed.ToString("g")}");
                    indexService.Remove(contentInstance, indexName);
                }
                else
                {
                    LogHelper.Info<ContentIndexer>($"- [{indexName}] Updating {NodeDisplayPath(contentInstance.Path)} : elapsed {_stopWatch.Elapsed.ToString("g")}");
                    indexService?.Index(contentInstance, indexName);
                }

                if (isRecursive && contentInstance.Children().Any())
                {
                    var children = contentInstance.Children().ToList();
                    if (children.Any())
                    {
                        LogHelper.Info<ContentIndexer>($"- [{indexName}] Updating children of {NodeDisplayPath(contentInstance.Path)} ({children.Count}) : elapsed {_stopWatch.Elapsed.ToString("g")}");
                        foreach (var child in children)
                        {
                            Publish(child, indexName, true);
                        }
                    }
                }
            }
        }

        private string NodeDisplayPath(string path)
        {
            return string.Join("/", path.Split(',')
                .Select(int.Parse)
                .Select(id => UmbracoContext.Current.ContentCache.GetById(id))
                .Where(content => content != null)
                .Select(content => $"{content.Name}({content.Id})")
                );
        } */
    }
}