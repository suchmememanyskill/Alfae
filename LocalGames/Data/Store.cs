namespace LocalGames.Data;

public class Store
{
    public List<LocalGame> LocalGames { get; set; } = new();
    public List<GenerationRules> GenerationRules { get; set; } = new();
}