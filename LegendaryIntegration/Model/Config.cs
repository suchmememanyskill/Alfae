using Newtonsoft.Json;

namespace LegendaryIntegration.Model
{
    public class Config
    {
        [JsonProperty("GameConfigs")]
        public Dictionary<string, ConfigItem> GameConfigs { get; set; } = new();
    }

    public class ConfigItem
    {
        [JsonProperty("AlwaysOffline")]
        public bool AlwaysOffline { get; set; } = false;
        [JsonProperty("AlwaysSkipUpdate")]
        public bool AlwaysSkipUpdate { get; set; } = false;
        [JsonProperty("AdditionalArgs")]
        public string AdditionalArgs { get; set; } = "";
    }
}
