namespace LocalGames.Data;

public class GenerationRules
{
    public string Name { get; set; } = "";
    public List<string> Extensions { get; set; } = new();
    public string Path { get; set; } = "";
    public string LocalGameName { get; set; } = "";
    public string AdditionalCliArgs { get; set; } = "";
}