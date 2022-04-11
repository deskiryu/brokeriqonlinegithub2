using Microsoft.Extensions.Diagnostics.HealthChecks;
using ReviewIt.Web.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReviewIt.Web.Server.Helper
{
    public class HealthCheck : IHealthCheck
    {
        IHealthService healthService;
        public HealthCheck(IHealthService healthService)
        {
            this.healthService = healthService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var healthCheckResultHealthy = await this.healthService.CanPingApi();

            if (healthCheckResultHealthy)
            {
                return HealthCheckResult.Healthy("A healthy result.");
            }

            return new HealthCheckResult(context.Registration.FailureStatus, "An unhealthy result.");
        }
    }
}