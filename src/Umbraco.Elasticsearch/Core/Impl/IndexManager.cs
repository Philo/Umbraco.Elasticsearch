using System.Threading.Tasks;
using Umbraco.Core.Logging;

namespace Umbraco.Elasticsearch.Core.Impl
{
    public class IndexManager : IIndexManager {
        public void Delete()
        {
            if (UmbracoSearchFactory.Client.IndexExists(i => i.Index(UmbracoSearchFactory.Client.Infer.DefaultIndex)).Exists)
            {
                UmbracoSearchFactory.Client.DeleteIndex(i => i.Index(UmbracoSearchFactory.Client.Infer.DefaultIndex));
                LogHelper.Info<IndexManager>(() => $"Search index '{UmbracoSearchFactory.Client.Infer.DefaultIndex}' has been deleted");
            }
        }

        public void Create(bool deleteExisting = false)
        {
            if (deleteExisting) Delete();

            if (
                !UmbracoSearchFactory.Client.IndexExists(i => i.Index(UmbracoSearchFactory.Client.Infer.DefaultIndex))
                    .Exists)
            {
                var strategy = UmbracoSearchFactory.GetIndexStrategy();
                strategy.Create();
                LogHelper.Info<IndexManager>(() => $"Search index '{UmbracoSearchFactory.Client.Infer.DefaultIndex}' has been created");

                Parallel.ForEach(UmbracoSearchFactory.GetContentIndexServices(), c => c.UpdateIndexTypeMapping());
                Parallel.ForEach(UmbracoSearchFactory.GetMediaIndexServices(), c => c.UpdateIndexTypeMapping());

                /*
                foreach (var indexService in UmbracoSearchFactory.GetContentIndexServices())
                {
                    indexService.UpdateIndexTypeMapping();
                }

                foreach (var indexService in UmbracoSearchFactory.GetMediaIndexServices())
                {
                    indexService.UpdateIndexTypeMapping();
                }*/
            }
        }
    }
}