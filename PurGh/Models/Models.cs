namespace PurGh
{
    using System;

    using Newtonsoft.Json;

    public class Settings
    {
        public string Owner { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
        public int CountToRetain { get; set; }

        [JsonIgnore]
        public readonly static string DefaultAppSettingsFile = $"{nameof(PurGh)}.json".GetFullPath();

        [JsonIgnore]
        public string AppSettingsFile { get; set; } = DefaultAppSettingsFile;
    }

    public class Artifacts
    {
        public int total_count { get; set; }

        [JsonProperty("artifacts")]
        public Artifact[] items { get; set; }
    }

    public class Artifact
    {
        public int id { get; set; }
        public string node_id { get; set; }
        public string name { get; set; }
        public int size_in_bytes { get; set; }
        public string url { get; set; }
        public string archive_download_url { get; set; }
        public bool expired { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime expires_at { get; set; }
    }

    public class WorkflowRuns
    {
        public int total_count { get; set; }

        [JsonProperty("workflow_runs")]
        public WorkflowRun[] items { get; set; }
    }

    public class WorkflowRun
    {        
        public int id { get; set; }
        public string name { get; set; }
        public string node_id { get; set; }
        public string head_branch { get; set; }
        public int run_number { get; set; }
        public string _event { get; set; }
        public string status { get; set; }
        public string conclusion { get; set; }
        public int workflow_id { get; set; }
        public string workflow_url { get; set; }
        public string artifacts_url { get; set; }
        public string url { get; set; }
        public object[] pull_requests { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}
