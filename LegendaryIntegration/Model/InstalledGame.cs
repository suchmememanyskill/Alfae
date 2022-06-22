using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LegendaryIntegration.Model
{
    public class InstalledGame
    {
        [JsonProperty("app_name")]
        public string AppName { get; set; }

        [JsonProperty("base_urls")]
        public List<Uri> BaseUrls { get; set; }

        [JsonProperty("can_run_offline")]
        public bool CanRunOffline { get; set; }

        [JsonProperty("egl_guid")]
        public string EglGuid { get; set; }

        [JsonProperty("executable")]
        public string Executable { get; set; }

        [JsonProperty("install_path")]
        public string InstallPath { get; set; }

        [JsonProperty("install_size")]
        public long InstallSize { get; set; }

        [JsonProperty("install_tags")]
        public List<string> InstallTags { get; set; }

        [JsonProperty("is_dlc")]
        public bool IsDlc { get; set; }

        [JsonProperty("launch_parameters")]
        public string LaunchParameters { get; set; }

        [JsonProperty("manifest_path")]
        public string ManifestPath { get; set; }

        [JsonProperty("needs_verification")]
        public bool NeedsVerification { get; set; }

        [JsonProperty("prereq_info")]
        public PreReqInfo PrereqInfo { get; set; }

        [JsonProperty("requires_ot")]
        public bool RequiresOt { get; set; }

        [JsonProperty("save_path")]
        public object SavePath { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
    
    public class PreReqInfo
    {
        [JsonProperty("args")]
        public string Args { get; set; }

        [JsonProperty("ids")]
        public List<string> Ids { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }
}
