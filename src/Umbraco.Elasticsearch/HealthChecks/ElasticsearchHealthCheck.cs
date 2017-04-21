using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Elasticsearch.Core;
using Umbraco.Web.HealthCheck;

namespace Umbraco.Elasticsearch.HealthChecks
{
    [HealthCheck("b6c8fe79-147b-448a-8310-d4f4796b9f9d", "Elasticsearch Health Check", Description = "Verifies the configured Elasticsearch cluster setup", Group = "Umbraco.Elasticsearch")]
    public class ElasticsearchHealthCheck : HealthCheck
    {
        public ElasticsearchHealthCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            var responding = CanCommunicateWithHost();
            yield return responding;
            if (responding.ResultType == StatusResultType.Success)
            {
                yield return VerifyElasticsearchVersion();
            }
        }

        private HealthCheckStatus VerifyElasticsearchVersion()
        {
            var versionNumber = Task.Run(UmbracoSearchFactory.GetElasticsearchVersionAsync).Result;

            var status = new HealthCheckStatus($"Elasticsearch version: <strong>{versionNumber}</strong>")
            {
                Description = "Elasticsearch cluster must be running at least version 5.0",
                ResultType = versionNumber.Major < 5 ? StatusResultType.Error : StatusResultType.Info
            };
            return status;
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            return null;
        }

        private HealthCheckStatus CanCommunicateWithHost()
        {
            var client = UmbracoSearchFactory.Client;
            var result = client.Ping(p => p.Human());
            
            return new HealthCheckStatus(result.IsValid ? "Configured host is up and responding" : result.OriginalException.Message)
            {
                ResultType = result.IsValid ? StatusResultType.Success : StatusResultType.Error
            };
        }
    }
}
