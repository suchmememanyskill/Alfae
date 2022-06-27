namespace LegendaryIntegration.Model
{
    public class InstalledGameList
    { 
        public Dictionary<string, InstalledGame> Games { get; set; }
        public List<InstalledGame> GetGamesAsList() => Games.Select((x) => x.Value).ToList();
    }
}
