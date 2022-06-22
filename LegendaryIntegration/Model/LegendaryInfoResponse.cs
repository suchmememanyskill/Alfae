using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryIntegration.Model
{
    public class LegendaryInfoResponse
    {
        [JsonProperty("game")]
        public Game Game { get; set; }

        [JsonProperty("manifest")]
        public Manifest Manifest { get; set; }
    }

    public partial class Game
    {
        [JsonProperty("app_name")]
        public string AppName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("platform_versions")]
        public Dictionary<string, string> PlatformVersions { get; set; }

        [JsonProperty("cloud_saves_supported")]
        public bool CloudSavesSupported { get; set; }

        [JsonProperty("cloud_save_folder")]
        public string CloudSaveFolder { get; set; }

        [JsonProperty("cloud_save_folder_mac")]
        public string CloudSaveFolderMac { get; set; }

        [JsonProperty("is_dlc")]
        public bool IsDlc { get; set; }
    }

    public partial class Manifest
    {
        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }

        [JsonProperty("feature_level")]
        public long FeatureLevel { get; set; }

        [JsonProperty("app_name")]
        public string AppName { get; set; }

        [JsonProperty("launch_exe")]
        public string LaunchExe { get; set; }

        [JsonProperty("launch_command")]
        public string LaunchCommand { get; set; }

        [JsonProperty("build_version")]
        public string BuildVersion { get; set; }

        [JsonProperty("build_id")]
        public string BuildId { get; set; }

        [JsonProperty("prerequisites")]
        public object Prerequisites { get; set; }

        [JsonProperty("install_tags")]
        public List<string> InstallTags { get; set; }

        [JsonProperty("num_files")]
        public long NumFiles { get; set; }

        [JsonProperty("num_chunks")]
        public long NumChunks { get; set; }

        [JsonProperty("disk_size")]
        public long DiskSize { get; set; }

        [JsonProperty("download_size")]
        public long DownloadSize { get; set; }

        [JsonIgnore]
        private readonly string[] gameSizes = { "B", "KB", "MB", "GB" };
        [JsonIgnore]
        public string DiskSizeReadable
        {
            get
            {
                int type = 0;
                double bytesLeft = DiskSize;
                while (bytesLeft >= 1024)
                {
                    type++;
                    bytesLeft /= 1024;
                }

                return $"{bytesLeft:0.00} {gameSizes[type]}";
            }
        }
        [JsonIgnore]
        public string DownloadSizeReadable
        {
            get
            {
                int type = 0;
                double bytesLeft = DownloadSize;
                while (bytesLeft >= 1024)
                {
                    type++;
                    bytesLeft /= 1024;
                }

                return $"{bytesLeft:0.00} {gameSizes[type]}";
            }
        }
    }
}
