using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Elasticsearch.Core;
using Umbraco.Elasticsearch.Core.Content.Impl;
using Umbraco.Elasticsearch.Core.Impl;
using Umbraco.Elasticsearch.Core.Media.Impl;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Umbraco.Elasticsearch.Admin.Api
{
    [PluginController("umbElasticsearch")]
    public class UmbElasticsearchIndexingController : UmbracoAuthorizedJsonController
    {
        private readonly IIndexManager _indexManager;
        private readonly string _indexName;

        public UmbElasticsearchIndexingController(IIndexManager indexManager)
        {
            _indexManager = indexManager;
        }

        public UmbElasticsearchIndexingController() : this(new IndexManager())
        {
            _indexName = UmbracoSearchFactory.Client.Infer.DefaultIndex;
        }

        [HttpGet]
        public IHttpActionResult MediaIndexServicesList()
        {
            var media = UmbracoSearchFactory.GetMediaIndexServices();

            return Ok(media.Select(x => new
            {
                x.DocumentTypeName,
                x.GetType().Name,
                Count = x.CountOfDocumentsForIndex(_indexName)
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
                Count = x.CountOfDocumentsForIndex(_indexName)
            }));
        }

        [HttpPost]
        public async Task DeleteIndexByName([FromBody] string indexName)
        {
            await _indexManager.DeleteIndexAsync(indexName);
        }

        [HttpPost]
        public async Task ActivateIndexByName([FromBody] string indexName)
        {
            await _indexManager.ActivateIndexAsync(indexName);
        }

        [HttpGet]
        public async Task<IHttpActionResult> IndicesInfo()
        {
            var info = await _indexManager.IndicesInfo();
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
            return await _indexManager.GetElasticsearchVersion();
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
            var mappings = await _indexManager.GetIndexMappingInfo(indexName);
            return Ok(mappings);
        }
    }
}
