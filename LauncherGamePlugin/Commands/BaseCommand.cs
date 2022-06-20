namespace LauncherGamePlugin.Commands;

public class BaseCommand
{
    public string Text { get; set; }

    public BaseCommand(string text)
    {
        Text = text;
    }
}