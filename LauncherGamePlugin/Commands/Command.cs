namespace LauncherGamePlugin.Commands;

public enum CommandType
{
    Separator,
    Text,
    Function,
    SubMenu,
}

public class Command
{
    public string? Text { get; set; }
    public Action? Action { get; set; }
    public CommandType Type { get; set; }
    public List<Command> SubCommands { get; set; } = new();

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

    public Command(string text, List<Command> subCommands)
    {
        Type = CommandType.SubMenu;
        Text = text;
        SubCommands = subCommands;
    }
}