namespace LauncherGamePlugin.Commands;

public enum CommandType
{
    Separator,
    Text,
    Function,
}

public class Command
{
    public string? Text { get; set; }
    public Action? Action { get; set; }
    public CommandType Type { get; set; }

    public Command()
    {
        Type = CommandType.Separator;
    }

    public Command(string text)
    {
        Type = CommandType.Text;
        Text = text;
    }

    public Command(string text, Action action)
    {
        Type = CommandType.Function;
        Text = text;
        Action = action;
    }
}