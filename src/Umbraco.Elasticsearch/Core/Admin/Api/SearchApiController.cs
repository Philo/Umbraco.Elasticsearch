using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Nest;
using Umbraco.Elasticsearch.Core.Content.Impl;
using Umbraco.Elasticsearch.Core.Impl;
using Umbraco.Elasticsearch.Core.Media.Impl;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Umbraco.Elasticsearch.Core.Admin.Api
{
    [PluginController("searchSection")]
    public class SearchApiController : UmbracoAuthorizedJsonController
    {
        private readonly IElasticClient _client;

        public SearchApiController(IElasticClient client)
        {
            _client = client;
        }

        public SearchApiController() : this(UmbracoSearchFactory.Client) { }

        [HttpGet]
        public async Task<Dictionary<string, object>> Stats()
        {
            var response = await _client.IndicesStatsAsync();
            if (response.Indices.ContainsKey(_client.Infer.DefaultIndex))
            {
                var indexStats = response.Indices[_client.Infer.DefaultIndex];
                return new Dictionary<string, object>()
                {
                    {"timestamp", DateTime.UtcNow.ToString("O") },
                    {"docCount", indexStats.Primaries.Documents.Count},
                    {"totalQueries", indexStats.Primaries.Search.QueryTotal}
                };
            }
            return new Dictionary<string, object>();
        }

        [HttpPost]
        public IHttpActionResult DeleteIndex()
        {
            var manager = new IndexManager();
            try
            {
                manager.Delete();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IHttpActionResult CreateIndex()
        {
            var manager = new IndexManager();
            try
            {
                manager.Create(true);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IHttpActionResult RebuildContentIndex()
        {
            var indexer = new ContentIndexer();
            indexer.Build();

            return Ok();
        }

        [HttpPost]
        public IHttpActionResult RebuildMediaIndex()
        {
            var indexer = new MediaIndexer();
            indexer.Build();

            return Ok();
        }
    }
}
