namespace LauncherGamePlugin.Commands;

public class ActionCommand : BaseCommand
{
    public Action Action { get; set; }
    
    public ActionCommand(string text, Action action) : base(text)
    {
        Action = action;
    }
}