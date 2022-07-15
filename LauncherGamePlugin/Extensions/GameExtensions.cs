using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Interfaces;

namespace LauncherGamePlugin.Extensions;

public static class GameExtensions
{
    public static string ReadableSize(this IGame game)
    {
        return game.Size!.Value.ReadableSize();
    }

    public static List<Command> GetCommands(this IGame game) => game.Source.GetGameCommands(game);
}