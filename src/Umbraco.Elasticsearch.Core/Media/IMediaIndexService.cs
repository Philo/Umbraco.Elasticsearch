using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Elasticsearch.Core.Media
{
    public interface IMediaIndexService<in TMedia> : IIndexService<TMedia> where TMedia : IMedia
    {
    }

    public interface IMediaIndexService : IMediaIndexService<IMedia> { }
}