using ItchIoIntegration.Service;
using LauncherGamePlugin.Interfaces;
using Newtonsoft.Json;

namespace ItchIoIntegration.Model;

public class Config
{
    public string ApiKey { get; set; } = "";
    public List<ItchGame> InstalledGames { get; set; } = new();
}