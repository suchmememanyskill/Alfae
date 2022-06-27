using Newtonsoft.Json;

namespace BottlesPlugin.Model;

public class Config
{
    [JsonProperty("import_programs")] public bool ImportPrograms { get; set; } = false;
}