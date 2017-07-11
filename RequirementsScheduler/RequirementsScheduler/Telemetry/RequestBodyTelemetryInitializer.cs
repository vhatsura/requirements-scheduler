using System.Net.Http;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace RequirementsScheduler.Telemetry
{
    public class RequestBodyTelemetryInitializer : ITelemetryInitializer
    {
        private IHttpContextAccessor ContextAccessor { get; }

        public RequestBodyTelemetryInitializer(IHttpContextAccessor contextAccessor)
        {
            ContextAccessor = contextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            var requestTelemetry = telemetry as RequestTelemetry;
            if (requestTelemetry != null &&
                requestTelemetry.Properties.ContainsKey("httpMethod") &&
                requestTelemetry.Properties["httpMethod"] == HttpMethod.Post.ToString())
            {
                if (ContextAccessor.HttpContext.Items.ContainsKey("body"))
                {
                    requestTelemetry.Properties.Add("body", ContextAccessor.HttpContext.Items["body"] as string);
                }
            }
        }
    }
}
