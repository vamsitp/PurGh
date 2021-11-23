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

        private string artifactsEndpoint;
        private string workflowRunsEndpoint;
        private string authHeaderValue;

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

        public async Task<Artifacts> GetArtifacts()
        {
            var result = await this.GetRequest(artifactsEndpoint).GetJsonAsync<Artifacts>();
            return result;
        }

        public async Task<List<int>> PurgeArtifacts(IEnumerable<Artifact> artifacts)
        {
            var results = new List<int>();
            foreach (var artifact in artifacts)
            {
                var result = await PurgeArtifact(artifact);
                results.Add(result);
            }

            return results;
        }

        public async Task<int> PurgeArtifact(Artifact artifact)
        {
            var result = await this.GetRequest(artifactsEndpoint + $"/{artifact.id}").DeleteAsync();
            return artifact.id;
        }

        public async Task<WorkflowRuns> GetWorkflowRuns()
        {
            var result = await this.GetRequest(workflowRunsEndpoint).GetJsonAsync<WorkflowRuns>();
            return result;
        }

        public async Task<List<int>> PurgeWorkflowRuns(IEnumerable<WorkflowRun> runs)
        {
            var results = new List<int>();
            foreach (var run in runs)
            {
                var result = await PurgeWorkflowRun(run);
                results.Add(result);
            }

            return results;
        }

        public async Task<int> PurgeWorkflowRun(WorkflowRun run)
        {
            var result = await this.GetRequest(workflowRunsEndpoint + $"/{run.id}").DeleteAsync();
            return run.id;
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
