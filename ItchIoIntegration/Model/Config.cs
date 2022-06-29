using ItchIoIntegration.Service;
using LauncherGamePlugin.Interfaces;
using Newtonsoft.Json;

namespace ItchIoIntegration.Model;

public class Config
{
    public string ApiKey { get; set; } = "";
    public List<ItchGame> InstalledGames { get; set; } = new();

    public void Save(IApp app)
    {
        string path = Path.Combine(app.ConfigDir, "itch.json");
        File.WriteAllText(path, JsonConvert.SerializeObject(this));
    }

    public static Config Load(IApp app)
    {
        string path = Path.Combine(app.ConfigDir, "itch.json");
        Config? c = new();
        if (File.Exists(path))
        {
            try
            {
                c = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
                if (c == null)
                    return new();
            }
            catch
            {
                return new();
            }

        }

        return c;
    }
}