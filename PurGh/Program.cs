namespace PurGh
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using ColoredConsole;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    internal class Program
    {
        [STAThread]
        public static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            try
            {
                // Credit: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.1
                // Credit: https://thecodebuzz.com/using-httpclientfactory-in-net-core-console-application/
                IConfiguration configuration = null;
                var appSettingsFile = string.IsNullOrWhiteSpace(args?.FirstOrDefault()) || !File.Exists(args?.FirstOrDefault()) ? $"{nameof(PurGh)}.json".GetFullPath() : args.FirstOrDefault();
                var builder = Host
                                .CreateDefaultBuilder(args)
                                .ConfigureAppConfiguration((hostContext, config) =>
                                {
                                    config
                                        .SetBasePath(string.Empty.GetFullPath())
                                        .AddJsonFile(appSettingsFile, optional: true, reloadOnChange: true);
                                    configuration = config.Build();
                                })
                                .ConfigureServices((hostContext, services) =>
                                {
                                    services
                                        .Configure<Settings>(configuration)
                                        .PostConfigure<Settings>(config => { config.AppSettingsFile = appSettingsFile; })
                                        .AddSingleton<PurgeService>()
                                        .AddHostedService<Worker>();
                                })
                                .UseConsoleLifetime();

                await builder.RunConsoleAsync(options => options.SuppressStatusMessages = true);
            }
            catch (Exception ex)
            {
                ColorConsole.WriteLine(ex.Message.White().OnRed());
            }
        }
    }
}
