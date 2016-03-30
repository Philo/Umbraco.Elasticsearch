using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace Umbraco.Elasticsearch.Core.Admin.Section
{
    /*
    [Application("searchSection", "searchSection", "icon-search", 15)]
    public class SearchApplication : IApplication { } */

    [PluginController("searchSection")]
    [Web.Trees.Tree("searchSection", "searchSectionTree", "Search")]
    public class SearchTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();
            if (UmbracoSearchFactory.IsSearchAvailable())
            {
                var client = UmbracoSearchFactory.Client;
                var indexName = client.Infer.DefaultIndex;
                if (id == Constants.System.Root.ToInvariantString())
                {
                    nodes.Add(CreateTreeNode("manage", id, queryStrings, indexName, "icon-ordered-list", true,
                        "searchSection/searchSectionTree/manage/" + indexName));
                }
                else if (id == "manage")
                {
                    nodes.Add(CreateTreeNode("info", id, queryStrings, "Info", "icon-settings color-green", false,
                        "searchSection/searchSectionTree/info/" + indexName));
                    nodes.Add(CreateTreeNode("rebuild", id, queryStrings, "Rebuild", "icon-sync color-orange", false,
                        "searchSection/searchSectionTree/rebuild/" + indexName));
                    nodes.Add(CreateTreeNode("delete", id, queryStrings, "Delete", "icon-delete color-red", false,
                        "searchSection/searchSectionTree/delete/" + indexName));
                }
            }
            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            return null;
        }
    }
}
