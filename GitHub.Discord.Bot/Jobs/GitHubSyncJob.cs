using System;
using System.Threading.Tasks;
using GitHub.Discord.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace GitHub.Discord.Bot.Jobs
{
    public class GitHubSyncJob : IJob
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<GitHubSyncJob> _logger;

        public GitHubSyncJob(IServiceProvider services)
        {
            _services = services;
            _logger = _services.GetRequiredService<ILoggerFactory>().CreateLogger<GitHubSyncJob>();
            _logger.LogInformation("GitHubSyncJob created");
        }
        
        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("[QUARTZ] GitHubSyncJob started");
            try
            {
                await _services.GetRequiredService<IIssueUpdateService>().UpdateReferencedIssues();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[QUARTZ] GitHubSyncJob failed");
            }
            _logger.LogInformation("[QUARTZ] GitHubSyncJob finished");
        }
    }
}