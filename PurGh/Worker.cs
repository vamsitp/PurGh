namespace PurGh
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
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
            ColorConsole.WriteLine("CountToRetain", ": ".Green(), this.settings.CountToRetain.ToString().DarkGray());

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
                        var results = await this.purger.GetArtifacts();
                        ColorConsole.WriteLine("artifacts: ", results.total_count.ToString().Green());
                    }
                    else if (key.Equals("r") || key.StartsWithIgnoreCase("run") || key.Equals("w") || key.StartsWithIgnoreCase("workflow"))
                    {
                        var results = await this.purger.GetWorkflowRuns();
                        ColorConsole.WriteLine("runs: ", results.total_count.ToString().Green());
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

        private static void PrintHelp()
        {
            ColorConsole.WriteLine(
                new[]
                {
                    "--------------------------------------------------".Green(),
                    "\nEnter ", "c".Green(), " to clear the console",
                    "\nEnter ", "q".Green(), " to quit",
                    "\nEnter ", "?".Green(), " to print this help"
                });
        }
    }
}
