using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BottlesPlugin.Model;

public class Program
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("removed", NullValueHandling = NullValueHandling.Ignore)] 
    public bool Removed { get; set; } = false;
}