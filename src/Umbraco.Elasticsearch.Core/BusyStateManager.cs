using System;
using System.Diagnostics;
using Umbraco.Core.Logging;

namespace Umbraco.Elasticsearch.Core
{
    public class BusyStateManager : IDisposable
    {
        public static bool IsBusy { get; private set; }

        public static string Message
        {
            get { return $"{_message} (Elapsed: {Elapsed.TotalSeconds.ToString("##.000")} seconds)"; }
            private set { _message = value; }
        }

        public static string IndexName { get; private set; }

        private static readonly Stopwatch Stopwatch = new Stopwatch();

        public static TimeSpan Elapsed => Stopwatch.Elapsed;
        private static readonly object Locker = new object();
        private static string _message;

        public static IDisposable Start(string message, string indexName)
        {
            if(IsBusy) throw new InvalidOperationException("Another index operation is currently in progress, please try again later");
            return new BusyStateManager(message, indexName);
        }

        private BusyStateManager(string message, string indexName)
        {
            LogHelper.Info<BusyStateManager>(message);
            lock (Locker)
            {
                Stopwatch.Restart();
                IsBusy = true;
                Message = message;
                IndexName = indexName;
            }
        }

        public static void UpdateMessage(string message)
        {
            LogHelper.Info<BusyStateManager>(message);
            lock (Locker)
            {
                Message = message;
            }
        }

        public void End()
        {
            LogHelper.Info<BusyStateManager>($"Busy state ended after {Elapsed.ToString("g")}");
            lock (Locker)
            {
                Stopwatch.Stop();
                IsBusy = false;
            }
        }

        public void Dispose()
        {
            End();
        }
    }
}