using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Nest;
using Umbraco.Elasticsearch.Core;
using Umbraco.Elasticsearch.Core.Content.Impl;
using Umbraco.Elasticsearch.Core.Impl;
using Umbraco.Elasticsearch.Core.Media.Impl;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Umbraco.Elasticsearch.Admin.Api
{
    [PluginController("umbElasticsearch")]
    public class SearchApiController : UmbracoAuthorizedJsonController
    {
        private readonly IElasticClient _client;
        private static Version _versionInfo;
        private readonly string _indexName;

        public SearchApiController(IElasticClient client)
        {
            _client = client;
        }

        public SearchApiController() : this(UmbracoSearchFactory.Client)
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
            await _client.DeleteIndexAsync(d => d.Index(indexName));
        }

        [HttpPost]
        public async Task ActivateIndexByName([FromBody] string indexName)
        {
            var indexAliasName = _client.Infer.DefaultIndex;
            await _client.AliasAsync(a => a
                .Remove(r => r.Alias(indexAliasName).Index($"{indexAliasName}*"))
                .Add(aa => aa.Alias(indexAliasName).Index(indexName))
                );
        }

        [HttpGet]
        public async Task<object> IndicesInfo()
        {
            var response = await _client.IndicesStatsAsync();

            return response.Indices.Where(x => x.Key.StartsWith(_indexName)).Select(x => new
            {
                Name = x.Key,
                DocCount = x.Value.Total.Documents.Count,
                Queries = x.Value.Total.Search.QueryTotal,
                SizeInBytes = x.Value.Total.Store.SizeInBytes,
                Status = GetStatus(x.Key) ? "Active" : ""
            });
        }

        private bool GetStatus(string indexName)
        {
            return _client.AliasExists(x => x.Index(indexName).Name(_indexName)).Exists;
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
            if (_versionInfo == null)
            {
                var info = await _client.RootNodeInfoAsync();
                if (info.IsValid)
                {
                    _versionInfo = new Version(info.Version.Number);
                }
            }
            return _versionInfo;
        }

        [HttpGet]
        public async Task<IHttpActionResult> Ping()
        {
            try
            {
                var result = await UmbracoSearchFactory.IsActiveAsync();
                return Ok(new
                {
                    active = result
                });
            }
            catch
            {
                return Ok(false);
            }
        }
    }
}
