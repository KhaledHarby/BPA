using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace SaaSZero.API.Jobs
{
	public class SampleCleanupJob : IJob
	{
		private readonly ILogger<SampleCleanupJob> _logger;
		public SampleCleanupJob(ILogger<SampleCleanupJob> logger)
		{
			_logger = logger;
		}

		public Task Execute(IJobExecutionContext context)
		{
			_logger.LogInformation("SampleCleanupJob executed at {Time}", DateTimeOffset.UtcNow);
			return Task.CompletedTask;
		}
	}
}