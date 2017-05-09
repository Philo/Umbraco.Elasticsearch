using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Http;
using umbraco.cms.businesslogic.contentitem;
using Umbraco.Core.Models;
using Umbraco.Elasticsearch.Core;
using Umbraco.Elasticsearch.Core.Content.Impl;
using Umbraco.Elasticsearch.Core.Impl;
using Umbraco.Elasticsearch.Core.Media.Impl;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Task = System.Threading.Tasks.Task;

namespace Umbraco.Elasticsearch.Admin.Api
{
    [PluginController("umbElasticsearch")]
    public class UmbElasticsearchIndexingController : UmbracoAuthorizedJsonController
    {
        private readonly IIndexManager indexManager;
        private readonly string indexName;

        public UmbElasticsearchIndexingController(IIndexManager indexManager)
        {
            this.indexManager = indexManager;
        }

        public UmbElasticsearchIndexingController() : this(new IndexManager())
        {
            indexName = UmbracoSearchFactory.ActiveIndexName;
        }

        [HttpGet]
        public IHttpActionResult MediaIndexServicesList()
        {
            var media = UmbracoSearchFactory.GetMediaIndexServices();

            return Ok(media.Select(x => new
            {
                x.DocumentTypeName,
                x.GetType().Name,
                Count = x.CountOfDocumentsForIndex(indexName)
            }));
        }

        [HttpGet]
        public IHttpActionResult ContentIndexServicesList()
        {
            var content = UmbracoSearchFactory.GetContentIndexServices();

            return Ok(content.Select(x => new
            {
                x.DocumentTypeName,
                x.GetType().Name,
                Count = x.CountOfDocumentsForIndex(indexName)
            }));
        }

        [HttpPost]
        public async Task DeleteIndexByName([FromBody] string indexName)
        {
            await indexManager.DeleteIndexAsync(indexName);
        }

        [HttpPost]
        public async Task ActivateIndexByName([FromBody] string indexName)
        {
            await indexManager.ActivateIndexAsync(indexName);
        }

        [HttpGet]
        public async Task<IHttpActionResult> IndicesInfo()
        {
            var info = await indexManager.IndicesInfo();
            return Ok(info);
        }
        
        [HttpPost]
        public async Task<IHttpActionResult> CreateIndex()
        {
            var manager = new IndexManager();
            try
            {
                await manager.CreateAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IHttpActionResult RebuildContentIndex([FromBody] string indexName)
        {
            var indexer = new ContentIndexer();
            indexer.Build(indexName);

            return Ok();
        }

        [HttpPost]
        public IHttpActionResult RebuildMediaIndex([FromBody] string indexName)
        {
            var indexer = new MediaIndexer();
            indexer.Build(indexName);

            return Ok();
        }

        [HttpGet]
        public async Task<IHttpActionResult> SearchVersionInfo()
        {
            var versionNumber = await GetVersionInfo();
            return Ok(new
            {
                version = versionNumber.ToString(3)
            });
        }

        private async Task<Version> GetVersionInfo()
        {
            return await indexManager.GetElasticsearchVersion();
        }

        [HttpGet]
        public async Task<IHttpActionResult> Ping()
        {
            try
            {
                var result = await UmbracoSearchFactory.IsActiveAsync();
                return Ok(new { active = result });
            }
            catch
            {
                return Ok(new { active = false });
            }
        }

        [HttpGet]
        public IHttpActionResult IsBusy()
        {
            return Ok(new {
                Busy = BusyStateManager.IsBusy,
                Message = BusyStateManager.Message,
                IndexName = BusyStateManager.IndexName,
                Elapsed = BusyStateManager.Elapsed.ToString(@"mm\ss\.ff")
            });
        }

        [HttpGet]
        public IHttpActionResult PluginVersionInfo()
        {
            return Ok(UmbracoSearchFactory.GetVersion());
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetIndexInfo([FromBody] string indexName)
        {
            var mappings = await indexManager.GetIndexMappingInfo(indexName);
            return Ok(mappings);
        }

        public class UpdateIndexNodeModel
        {
            public int NodeId { get; set; }
            public PublishedItemType NodeType { get; set; }
        }

        [HttpPost]
        public IHttpActionResult UpdateIndexNode(UpdateIndexNodeModel model)
        {
            switch (model.NodeType)
            {
                case PublishedItemType.Content:
                    if (ReindexContentNode(model.NodeId))
                    {
                        return Ok();
                    }
                    break;
                case PublishedItemType.Media:
                    if (ReindexMediaNode(model.NodeId))
                    {
                        return Ok();
                    }
                    break;
            }

            return BadRequest();
        }

        private bool ReindexMediaNode(int nodeId)
        {
            var mediaNode = ApplicationContext.Services.MediaService.GetById(nodeId);
            if (mediaNode != null)
            {
                var mediaIndexService = UmbracoSearchFactory.GetMediaIndexService(mediaNode);
                if (mediaIndexService != null)
                {
                    mediaIndexService.Index(mediaNode, UmbracoSearchFactory.ActiveIndexName);
                    return true;
                }
            }
            return false;
        }

        private bool ReindexContentNode(int nodeId)
        {
            var contentNode = ApplicationContext.Services.ContentService.GetById(nodeId);
            if (contentNode != null)
            {
                var contentIndexService = UmbracoSearchFactory.GetContentIndexService(contentNode);
                if (contentIndexService != null)
                {
                    contentIndexService.Index(contentNode, UmbracoSearchFactory.ActiveIndexName);
                    return true;
                }
            }
            return false;
        }
    }
}
