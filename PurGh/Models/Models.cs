namespace PurGh
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class Settings
    {
        public string Owner { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
        public Dictionary<string, int> ItemsCountToRetain { get; set; } = new Dictionary<string, int> { { "All", 5 } };

        [JsonIgnore]
        public readonly static string DefaultAppSettingsFile = $"{nameof(PurGh)}.json".GetFullPath();

        [JsonIgnore]
        public string AppSettingsFile { get; set; } = DefaultAppSettingsFile;
    }

    public class PurgeEntity
    {
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string node_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class PurgeEntities<T> 
        where T : PurgeEntity
    {
        public int total_count { get; set; }
        public virtual T[] items { get; set; }
    }

    public class Artifacts : PurgeEntities<Artifact>
    {
        [JsonProperty("artifacts")]
        public override Artifact[] items { get; set; }
    }

    public class Artifact : PurgeEntity
    {
        public int size_in_bytes { get; set; }
        public string archive_download_url { get; set; }
        public bool expired { get; set; }
        public DateTime expires_at { get; set; }
    }

    public class WorkflowRuns : PurgeEntities<WorkflowRun>
    {
        [JsonProperty("workflow_runs")]
        public override WorkflowRun[] items { get; set; }
    }

    public class WorkflowRun : PurgeEntity
    {
        public string head_branch { get; set; }
        public int run_number { get; set; }
        public string _event { get; set; }
        public string status { get; set; }
        public string conclusion { get; set; }
        public int workflow_id { get; set; }
        public string workflow_url { get; set; }
        public string artifacts_url { get; set; }
        public object[] pull_requests { get; set; }
    }
}
