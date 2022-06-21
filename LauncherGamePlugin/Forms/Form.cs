using LauncherGamePlugin.Interfaces;

namespace LauncherGamePlugin.Forms;

public class Form
{
    public List<FormEntry> FormEntries { get; set; }
    public void SetContainingForm() => FormEntries.ForEach(x => x.ContainingForm = this);

    public Func<Task<byte[]?>>? Background { get; set; } = null;
    public IGame? Game { get; set; } = null;

    public Form(List<FormEntry> entries)
    {
        FormEntries = entries;
        SetContainingForm();
    }

    public string? GetValue(string name) => FormEntries.Find(x => x.Name == name)?.Value;
}