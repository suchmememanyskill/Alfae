namespace LauncherGamePlugin.Forms;

public class Form
{
    public List<FormEntry> FormEntries { get; set; }
    public void SetContainingForm() => FormEntries.ForEach(x => x.ContainingForm = this);

    public Form(List<FormEntry> entries)
    {
        FormEntries = entries;
        SetContainingForm();
    }
}