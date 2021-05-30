using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GitHub.Discord.Bot.Cfg;
using GitHub.Discord.Bot.Data.Context;
using GitHub.Discord.Bot.Jobs;
using GitHub.Discord.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Serilog;
#if !DEBUG
using Serilog.Events;
#endif
using Serilog.Extensions.Logging;

namespace GitHub.Discord.Bot
{
    internal class Bot : IDisposable
    {
        private Bot()
        {
            _services = ConfigureServices();
            _logger = _services.GetService<ILoggerFactory>().CreateLogger<Bot>();
        }

        private readonly IServiceProvider _services;
        private readonly ILogger<Bot> _logger;
        private const string ConfigFile = "GitHub.Discord.Bot.Config.json";

        internal static readonly BaseConfiguration BaseConfiguration =
            JsonConvert.DeserializeObject<BaseConfiguration>(File.ReadAllText(ConfigFile));

        internal static void Main(string[] args) => new Bot().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            _logger.LogInformation("Starting ...");
            DiscordSocketClient client = _services.GetRequiredService<DiscordSocketClient>();
            _logger.LogInformation("Created Discord client: {@DiscordSocketClient}", client);
#if DEBUG
            _logger.LogInformation("Running application in DEBUG mode");
#endif
            _logger.LogInformation("Logging in ...");
            await client.LoginAsync(TokenType.Bot, BaseConfiguration.DiscordToken);
            _logger.LogInformation("Starting client ...");
            await client.StartAsync();
            _logger.LogInformation("Loading commands ...");
            await _services.GetRequiredService<ICommandHandlingService>().InitializeAsync();
            _logger.LogInformation("Starting scheduler ...");
            IScheduler scheduler = await _services.GetRequiredService<ISchedulerFactory>().GetScheduler();
            await scheduler.Start();
            _logger.LogInformation("Startup sequence complete. Awaiting user interaction ...");
            await Task.Delay(Timeout.Infinite);
            await scheduler.Shutdown();
            _logger.LogInformation("Shutting down ...");
        }

        private static ServiceProvider ConfigureServices()
        {
            LoggerProviderCollection providers = new();
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.File("GitHub.Discord.Bot.log")
#else
                .WriteTo.Sentry(o =>
                {
                    o.Dsn = BaseConfiguration.SentryToken;
                    o.MinimumBreadcrumbLevel = LogEventLevel.Information;
                    o.MinimumEventLevel = LogEventLevel.Warning;
                })
                .WriteTo.File("GitHub.Discord.Bot.log", LogEventLevel.Information)
#endif
                .WriteTo.Providers(providers)
                .CreateLogger();

            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<ICommandHandlingService, CommandHandlingService>()
                .AddSingleton<ILoggingService, LoggingService>()
                .AddSingleton<IGitHubService, GitHubService>()
                .AddSingleton<IIssueUpdateService, IssueUpdateService>()
                .AddSingleton<ILoggerFactory>(sc =>
                {
                    LoggerProviderCollection providerCollection = sc.GetService<LoggerProviderCollection>();
                    SerilogLoggerFactory factory = new(null, true, providerCollection);

                    foreach (ILoggerProvider provider in sc.GetServices<ILoggerProvider>())
                    {
                        factory.AddProvider(provider);
                    }

                    return factory;
                })
                .AddDbContext<BotContext>()
                .AddQuartz(q =>
                {
                    q.SchedulerId = "Scheduler-Core";
                    q.UseMicrosoftDependencyInjectionJobFactory();
                    q.UseSimpleTypeLoader();
                    q.UseInMemoryStore();
                    q.UseDefaultThreadPool(tp => { tp.MaxConcurrency = 10; });
                    q.ScheduleJob<GitHubSyncJob>(trigger => trigger
                        .WithIdentity("GitHub Sync Trigger")
                        .WithSimpleSchedule(x =>
                            x.WithIntervalInSeconds(Bot.BaseConfiguration.GitHubSyncInterval).RepeatForever())
                    );
                })
                .AddQuartzHostedService(options =>
                {
                    options.WaitForJobsToComplete = true;
                })
                .BuildServiceProvider();
        }

        public void Dispose()
        {
            if (_services is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}