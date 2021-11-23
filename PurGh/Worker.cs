namespace PurGh
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    using ColoredConsole;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class Worker : BackgroundService
    {
        private Settings settings;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<Worker> logger;
        private readonly PurgeService purger;
        private readonly IHostApplicationLifetime appLifetime;

        public Worker(IServiceScopeFactory serviceScopeFactory, IOptionsMonitor<Settings> settingsMonitor, ILogger<Worker> logger, IServiceProvider services, IHostApplicationLifetime appLifetime)
        {
            this.settings = settingsMonitor.CurrentValue;
            settingsMonitor.OnChange(changedSettings => { this.settings = changedSettings; });
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
            this.purger = services.GetService<PurgeService>();
            this.appLifetime = appLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            ColorConsole.WriteLine("--------------------------------------------------".Green(), "\nLoaded".Gray(), ": ".Green(), this.settings.AppSettingsFile.DarkGray());
            PrintHelp();

            ColorConsole.WriteLine("\nOwner", ": ".Green(), this.settings.Owner.DarkGray());
            ColorConsole.WriteLine("Name", ": ".Green(), this.settings.Name.DarkGray());
            ColorConsole.WriteLine("Token", ": ".Green(), (string.IsNullOrWhiteSpace(this.settings.Token) ? string.Empty : "********************").DarkGray());
            ColorConsole.WriteLine("ItemsCountToRetain", ": ".Green(), string.Join(", ", this.settings.ItemsCountToRetain.Select(x => $"{(string.IsNullOrWhiteSpace(x.Key) ? "All" : x.Key)}:{x.Value}")).TrimEnd(new[] { ',', ' ' }).DarkGray());

            do
            {
                try
                {
                    ColorConsole.Write("\n> ".Green());
                    var key = Console.ReadLine()?.Trim();
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        PrintHelp();
                        continue;
                    }
                    else if (key.EqualsIgnoreCase("q") || key.EqualsIgnoreCase("quit") || key.EqualsIgnoreCase("exit") || key.EqualsIgnoreCase("close"))
                    {
                        break;
                    }
                    else if (key.Equals("?") || key.EqualsIgnoreCase("help"))
                    {
                        PrintHelp();
                    }
                    else if (key.EqualsIgnoreCase("c") || key.EqualsIgnoreCase("cls") || key.EqualsIgnoreCase("clear"))
                    {
                        Console.Clear();
                    }
                    else if (key.Equals("a") || key.StartsWithIgnoreCase("artifact"))
                    {
                        await this.PurgeAsync<Artifacts, Artifact>();                        
                    }
                    else if (key.Equals("r") || key.StartsWithIgnoreCase("run") || key.Equals("w") || key.StartsWithIgnoreCase("workflow"))
                    {
                        await this.PurgeAsync<WorkflowRuns, WorkflowRun>();
                    }
                }
                catch (Exception ex)
                {
                    ColorConsole.WriteLine(ex.Message.White().OnRed());
                }
            }
            while (!stopToken.IsCancellationRequested);
            this.appLifetime.StopApplication();
        }

        private async Task PurgeAsync<TList, TEntity>()
            where TList : PurgeEntities<TEntity>
            where TEntity : PurgeEntity
        {
            var results = await this.purger.GetAll<TList, TEntity>();
            ColorConsole.WriteLine($"\n{typeof(TList).Name}: ", (results?.Count.ToString() ?? string.Empty).Green());
            var groups = results.GroupBy(x => x.name);
            foreach (var group in groups)
            {
                var countToRetain = this.settings.ItemsCountToRetain.ContainsKey(group.Key) ? this.settings.ItemsCountToRetain[group.Key] : (this.settings.ItemsCountToRetain.ContainsKey("All") ? this.settings.ItemsCountToRetain["All"] : (this.settings.ItemsCountToRetain.ContainsKey(string.Empty) ? this.settings.ItemsCountToRetain[string.Empty] : -1));
                if (countToRetain >= 0)
                {
                    var items = group.OrderByDescending(r => r.created_at);
                    var itemsToRetain = items.Take(countToRetain);
                    foreach (var item in itemsToRetain)
                    {
                        ColorConsole.WriteLine($"{typeof(TEntity).Name} to retain: ", item.name, " / ".Green(), item.created_at.ToString("ddMMMyy hh:mm:ss tt"), " / ".Green(), item.id.ToString());
                    }

                    await this.purger.PurgeAll(items.Skip(countToRetain));
                }
            }

            results = await this.purger.GetAll<TList, TEntity>();
            ColorConsole.WriteLine($"{typeof(TList).Name}: ", (results?.Count.ToString() ?? string.Empty).Green());
        }

        private static void PrintHelp()
        {
            ColorConsole.WriteLine(
                new[]
                {
                    "--------------------------------------------------".Green(),
                    "\nEnter ", "a".Green(), " to purge/delete Workflow-artifacts",
                    "\nEnter ", "r".Green(), " to purge/delete Workflow-runs",
                    "\nEnter ", "c".Green(), " to clear the console",
                    "\nEnter ", "q".Green(), " to quit",
                    "\nEnter ", "?".Green(), " to print this help",
                });
        }
    }
}
