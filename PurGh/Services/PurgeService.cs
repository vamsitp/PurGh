namespace PurGh
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ColoredConsole;

    using Flurl.Http;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    internal class PurgeService
    {
        private const string AuthHeader = "Authorization";

        private const string AcceptHeader = "Accept";
        private const string AcceptHeaderValue = "application/vnd.github.v3+json";

        private const string UserAgentHeader = "User-Agent";
        private const string UserAgentHeaderValue = "request";

        private const string ArtifactsEndpoint = "https://api.github.com/repos/{0}/{1}/actions/artifacts";
        private const string WorkflowRunsEndpoint = "https://api.github.com/repos/{0}/{1}/actions/runs";
        private const string AuthHeaderValue = "token {0}";

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

        public async Task<T> GetAll<T>()
        {
            var result = await this.GetRequest(typeof(T) == typeof(Artifacts) ? artifactsEndpoint : workflowRunsEndpoint).GetJsonAsync<T>();
            return result;
        }

        public async Task<List<int>> PurgeAll<T>(IEnumerable<T> items)
            where T : PurgeEntity
        {
            var results = new List<int>();
            foreach (var item in items)
            {
                var result = await Purge(item);
                results.Add(result);
            }

            return results;
        }

        public async Task<int> Purge<T>(T item)
            where T : PurgeEntity
        {
            var result = await this.GetRequest((typeof(T) == typeof(Artifact) ? artifactsEndpoint : workflowRunsEndpoint) + $"/{item.id}").DeleteAsync();
            return item.id;
        }

        public IFlurlRequest GetRequest(string url)
        {
            return url.WithHeader(AuthHeader, authHeaderValue).WithHeader(UserAgentHeader, UserAgentHeaderValue).WithHeader(AcceptHeader, AcceptHeaderValue);
        }

        private void Init()
        {
            this.artifactsEndpoint = string.Format(ArtifactsEndpoint, settings.Owner, settings.Name);
            this.workflowRunsEndpoint = string.Format(WorkflowRunsEndpoint, settings.Owner, settings.Name);
            this.authHeaderValue = string.Format(AuthHeaderValue, settings.Token);
        }
    }
}
