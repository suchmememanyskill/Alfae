using GogIntegration.Requests;
using Newtonsoft.Json;

namespace GogIntegration.Model;

public class Config
{
    public GogApiAuth? Auth { get; set; }

    public void Save(string configPath)
    {
        File.WriteAllText(configPath, JsonConvert.SerializeObject(this));
    }
}