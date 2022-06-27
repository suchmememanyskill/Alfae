using Newtonsoft.Json;

namespace BottlesPlugin.Model;

public class Bottle
{
    [JsonProperty("Name")]
    public string Name { get; set; }

    [JsonProperty("External_Programs")] 
    public Dictionary<string, Program> Programs { get; set; }
}