using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LegendaryIntegration.Model
{
    public class LegendaryStatusResponse
    {
        [JsonProperty("account")]
        public string AccountName { get; set; }
        [JsonProperty("games_available")]
        public int GamesAvailable { get; set; }
        [JsonProperty("games_installed")]
        public int GamesInstalled { get; set; }
        [JsonProperty("egl_sync_enabled")]
        public bool EglSyncEnabled { get; set; }
        [JsonProperty("config_directory")]
        public string ConfigDirectory { get; set; }

        public bool IsLoggedIn() => AccountName != "<not logged in>";
    }
}
