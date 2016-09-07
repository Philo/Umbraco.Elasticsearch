using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Elasticsearch.Core.Utils
{
    public enum IndexingStatusOption
    {
        Unknown,
        InProgress,
        Success,
        Error
    }

    public class IndexingStatus
    {
        public IndexingStatusOption Status { get; set; } = IndexingStatusOption.Unknown;
        public string Message { get; set; } = "";
    }

    public static class UmbracoEntityIndexingStateExtensions
    {
        private const string Key = "umbElasticsearchIndexingStatus";
        public static void SetIndexingStatus(this IUmbracoEntity entity, IndexingStatusOption status, string message)
        {
            if (!entity.AdditionalData.ContainsKey(Key))
            {
                entity.AdditionalData.Add(Key, new IndexingStatus()
                {
                    Status = status,
                    Message = message
                });
            }
            else
            {
                var state = entity.GetIndexingStatus();
                state.Status = status;
                state.Message = message;
            }
        }

        public static bool IndexSuccess(this IUmbracoEntity entity)
        {
            return entity.GetIndexingStatus().Status == IndexingStatusOption.Success;
        }

        public static bool IndexError(this IUmbracoEntity entity)
        {
            return entity.GetIndexingStatus().Status == IndexingStatusOption.Error;
        }

        public static IndexingStatus GetIndexingStatus(this IUmbracoEntity entity)
        {
            if (!entity.AdditionalData.ContainsKey(Key))
            {
                entity.AdditionalData.Add(Key, new IndexingStatus());
            }
            return entity.AdditionalData[Key] as IndexingStatus;
        }

        public static void ClearIndexingStatus(this IUmbracoEntity entity)
        {
            if (entity.AdditionalData.ContainsKey(Key))
            {
                entity.AdditionalData.Remove(Key);
            }
        }
    }
}
