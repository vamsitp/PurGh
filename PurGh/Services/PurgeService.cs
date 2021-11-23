namespace PurGh
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ColoredConsole;

    using Flurl.Http;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    internal class PurgeService
    {
        private static readonly Dictionary<string, string> DefaultHeaders = new Dictionary<string, string> { { "Accept", "application/vnd.github.v3+json" }, { "User-Agent", "request" } };
        private const string AuthHeader = "Authorization";
        private const string AuthHeaderValue = "token {0}";

        private const string ArtifactsEndpoint = "https://api.github.com/repos/{0}/{1}/actions/artifacts";
        private const string WorkflowRunsEndpoint = "https://api.github.com/repos/{0}/{1}/actions/runs";
        private const int DefaultPageCount = 100; //30

        private Settings settings;
        private readonly ILogger<Worker> logger;

        internal string artifactsEndpoint;
        internal string workflowRunsEndpoint;
        internal string authHeaderValue;

        public PurgeService(IOptionsMonitor<Settings> settingsMonitor, ILogger<Worker> logger)
        {
            this.settings = settingsMonitor.CurrentValue;
            Init();
            settingsMonitor.OnChange(changedSettings =>
            {
                this.settings = changedSettings;
                Init();
            });
            this.logger = logger;
        }

        public async Task<List<TEntity>> GetAll<TList, TEntity>()
            where TList : PurgeEntities<TEntity>
            where TEntity : PurgeEntity
        {
            var items = new List<TEntity>();
            var result = default(TList);
            var page = 1;
            do
            {
                var url = typeof(TList) == typeof(Artifacts) ? artifactsEndpoint : workflowRunsEndpoint;
                result = await this.GetRequest(url + $"?page={page++}&per_page={DefaultPageCount}").GetJsonAsync<TList>();
                ColorConsole.Write($".".Green()); // {result.items.Count}
                if (result?.items?.Count > 0)
                {
                    items.AddRange(result.items);
                }
            }
            while (result.total_count > items.Count); // + DefaultPageCount
            ColorConsole.WriteLine();
            return items;
        }

        public async Task<List<int>> PurgeAll<T>(IEnumerable<T> items)
            where T : PurgeEntity
        {
            var results = new List<int>();
            if (items?.Any() == true)
            {
                ColorConsole.Write($"\nPurge ", $"{items.Count()} ({items?.FirstOrDefault()?.name})".Red(), " items", " (Y/N) ".Green());
                var response = this.settings.QuiteMode == true ? "Y" : Console.ReadLine();
                if (response.EqualsIgnoreCase("Y"))
                {
                    foreach (var item in items)
                    {
                        var result = await Purge(item);
                        results.Add(result);
                    }
                }
            }

            ColorConsole.WriteLine();
            return results;
        }

        public async Task<int> Purge<T>(T item)
            where T : PurgeEntity
        {
            var result = await this.GetRequest((typeof(T) == typeof(Artifact) ? artifactsEndpoint : workflowRunsEndpoint) + $"/{item.id}").DeleteAsync();
            ColorConsole.Write($".".Red()); // {result}
            return item.id;
        }

        public IFlurlRequest GetRequest(string url)
        {
            return url.WithHeader(AuthHeader, authHeaderValue).WithHeaders(DefaultHeaders);
        }

        private void Init()
        {
            this.artifactsEndpoint = string.Format(ArtifactsEndpoint, settings.Owner, settings.Name);
            this.workflowRunsEndpoint = string.Format(WorkflowRunsEndpoint, settings.Owner, settings.Name);
            this.authHeaderValue = string.Format(AuthHeaderValue, settings.Token);
        }
    }
}
