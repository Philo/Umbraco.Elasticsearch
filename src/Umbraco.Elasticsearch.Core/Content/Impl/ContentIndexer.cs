using System;
using System.Diagnostics;
using Umbraco.Core.Logging;

namespace Umbraco.Elasticsearch.Core.Content.Impl
{
    public class ContentIndexer : IEntityIndexer
    {
        private readonly Stopwatch _stopWatch;

        public ContentIndexer()
        {
            _stopWatch = new Stopwatch();
        }

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
    }
}