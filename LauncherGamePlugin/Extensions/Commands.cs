using LauncherGamePlugin.Commands;

namespace LauncherGamePlugin.Extensions;

public static class Commands
{
    public static ActionCommand? GetActionCommand(this BaseCommand command) => command as ActionCommand;
    public static bool IsActionCommand(this BaseCommand command) => command.GetActionCommand() != null;
    public static bool IsSeperatorCommand(this BaseCommand command) => (command as SeparatorCommand) != null;
}