using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Elasticsearch.Core.Config;
using Umbraco.Elasticsearch.Core.Utils;
using Umbraco.Web.Cache;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;
using Umbraco.Web.UI.JavaScript;

namespace Umbraco.Elasticsearch.Core.EventHandlers
{
    public abstract class SearchApplicationEventHandler : SearchApplicationEventHandler<FromConfigSearchSettings>
    {
        protected SearchApplicationEventHandler() : base(new FromConfigSearchSettings())
        {
        }
    }

    public abstract partial class SearchApplicationEventHandler<TSearchSettings> : ApplicationEventHandler
        where TSearchSettings : ISearchSettings
    {
        protected SearchApplicationEventHandler(TSearchSettings searchSettings)
        {
            SearchSettings<TSearchSettings>.Set(searchSettings);
        }

        protected sealed override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            DashboardHelper.EnsureSection("umbElasticsearch", "Elasticsearch", "/App_Plugins/umbElasticsearch/umbElasticsearch.html");

            InstallServerVars();
        }

        protected sealed override void ApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            LogHelper.Info<SearchApplicationEventHandler<TSearchSettings>>(() => "Initialising configuration for Elasticsearch integration");
            Initialise(SearchSettings<TSearchSettings>.Current);
        }

        protected sealed override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Published += ContentService_Published;
            ContentService.UnPublished += ContentServiceOnUnPublished;
            ContentService.Trashed += ContentServiceOnTrashed;
            ContentService.Moved += ContentServiceOnMoved;
            ContentService.Copied += ContentServiceOnCopied;
            ContentService.RolledBack += ContentServiceOnRolledBack;

            MediaService.Moved += MediaServiceOnMoved;
            MediaService.Saved += MediaServiceOnSaved;
            MediaService.Deleted += MediaServiceOnDeleted;

            if (!SearchSettings<TSearchSettings>.Current.GetAdditionalData(UmbElasticsearchConstants.Configuration.DisableContentCacheUpdatedEventHook, false))
            {
                CacheRefresherBase<PageCacheRefresher>.CacheUpdated += CacheRefresherBaseOnCacheUpdated;
            }
            else
            {
                LogHelper.Info<SearchApplicationEventHandler<TSearchSettings>>("Disabled content cache update event hook");
            }

            foreach (var service in RegisterContentIndexingServices())
            {
                UmbracoSearchFactory.RegisterContentIndexService(service, service.ShouldIndex);
            }

            foreach (var service in RegisterMediaIndexingServices())
            {
                UmbracoSearchFactory.RegisterMediaIndexService(service, service.ShouldIndex);
            }

            AddReIndexForKnownDocumentTypes();
        }

        private void AddReIndexForKnownDocumentTypes()
        {
            if (SearchSettings<TSearchSettings>.Current.GetAdditionalData(UmbElasticsearchConstants.Configuration.EnableNodeLevelReIndex, true))
            {
                TreeControllerBase.MenuRendering += (sender, args) =>
                {
                    if (int.TryParse(args.NodeId, out int nodeId))
                    {
                        if (sender.TreeAlias == Constants.Trees.Content)
                        {
                            var node = sender.Umbraco.TypedContent(nodeId);
                            var contentService = UmbracoSearchFactory.GetContentIndexService(node.DocumentTypeAlias);
                            if (contentService != null)
                            {
                                var item = new MenuItem("umbElasticsearch.reindex", "Re-Index")
                                {
                                    Icon = "sync",
                                    SeperatorBefore = true
                                };
                                item.LaunchDialogView($"/App_Plugins/{UmbElasticsearchConstants.Configuration.Prefix}/updateIndexNode.html", $"Re-index '{node.Name}' Now");
                                args.Menu.Items.Add(item);
                            }
                        }
                        else if (sender.TreeAlias == Constants.Trees.Media)
                        {
                            var node = sender.Umbraco.TypedMedia(nodeId);
                            var mediaService = UmbracoSearchFactory.GetMediaIndexService(node.DocumentTypeAlias);
                            if (mediaService != null)
                            {
                                var item = new MenuItem("umbElasticsearch.reindex", "Re-Index")
                                {
                                    Icon = "sync",
                                    SeperatorBefore = true
                                };
                                item.LaunchDialogView($"/App_Plugins/{UmbElasticsearchConstants.Configuration.Prefix}/updateIndexNode.html", $"Re-index '{node.Name}' Now");
                                args.Menu.Items.Add(item);
                            }
                        }
                    }
                };
            }
        }

        private void CacheRefresherBaseOnCacheUpdated(PageCacheRefresher sender, CacheRefresherEventArgs cacheRefresherEventArgs)
        {
            if (cacheRefresherEventArgs.MessageType == MessageType.RefreshByInstance)
            {
                var content = cacheRefresherEventArgs.MessageObject as IContent;
                if (content != null && !content.IndexSuccess() && content.CreateDate == content.UpdateDate)
                {
                    LogHelper.Debug<SearchApplicationEventHandler<TSearchSettings>>($"First time content publishing via cache refresher for '{content.Name}'");
                    IndexContentCore(new[] { content }, new EventMessages());
                }
            }
        }

        private void ContentServiceOnRolledBack(IContentService sender, RollbackEventArgs<IContent> rollbackEventArgs)
        {
            IndexContentCore(rollbackEventArgs.Entity.AsEnumerableOfOne().ToList(), rollbackEventArgs.Messages);
        }

        private void ContentServiceOnCopied(IContentService sender, CopyEventArgs<IContent> copyEventArgs)
        {
            IndexContentCore(copyEventArgs.Copy.AsEnumerableOfOne().ToList(), copyEventArgs.Messages);
        }

        #region Event Handlers
        private void ContentServiceOnUnPublished(IPublishingStrategy sender, PublishEventArgs<IContent> publishEventArgs)
        {
            RemoveContentCore(publishEventArgs.PublishedEntities.ToList(), publishEventArgs.Messages);
        }

        private void ContentServiceOnTrashed(IContentService sender, MoveEventArgs<IContent> moveEventArgs)
        {
            RemoveContentCore(moveEventArgs.MoveInfoCollection.Select(x => x.Entity).ToList(), moveEventArgs.Messages);
        }

        private void ContentService_Published(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            var entitiesAwaitingIndex = e.PublishedEntities.Where(x => !x.IndexSuccess()).ToList();
            if (entitiesAwaitingIndex.Any())
            {
                IndexContentCore(entitiesAwaitingIndex, e.Messages);
            }
        }

        private void ContentServiceOnMoved(IContentService sender, MoveEventArgs<IContent> moveEventArgs)
        {
            IndexContentCore(moveEventArgs.MoveInfoCollection.Select(x => x.Entity).ToList(), moveEventArgs.Messages);
        }

        private void MediaServiceOnMoved(IMediaService sender, MoveEventArgs<IMedia> moveEventArgs)
        {
            IndexMediaCore(moveEventArgs.MoveInfoCollection.Select(x => x.Entity).ToList(), moveEventArgs.Messages);
        }

        private void MediaServiceOnDeleted(IMediaService sender, DeleteEventArgs<IMedia> deleteEventArgs)
        {
            RemoveMediaCore(deleteEventArgs.DeletedEntities.ToList(), deleteEventArgs.Messages);
        }

        private void MediaServiceOnSaved(IMediaService sender, SaveEventArgs<IMedia> saveEventArgs)
        {
            IndexMediaCore(saveEventArgs.SavedEntities.ToList(), saveEventArgs.Messages);
        }

        private static void InstallServerVars()
        {
            ServerVariablesParser.Parsing += (sender, serverVars) =>
            {
                if (!serverVars.ContainsKey("umbracoPlugins"))
                    throw new Exception("Missing umbracoPlugins.");
                var umbracoPlugins = serverVars["umbracoPlugins"] as Dictionary<string, object>;
                if (umbracoPlugins == null)
                    throw new Exception("Invalid umbracoPlugins");

                umbracoPlugins["umbElasticsearch"] = SearchSettings<TSearchSettings>.Current;
            };
        }

        #endregion
    }
}