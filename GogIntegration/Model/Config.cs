using GogIntegration.Requests;
using Newtonsoft.Json;

namespace GogIntegration.Model;

public class Config
{
    public GogApiAuth? Auth { get; set; }
    public List<GogGame> InstalledGames { get; set; } = new();
}