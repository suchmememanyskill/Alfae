using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Interfaces;

namespace LauncherGamePlugin.Extensions;

public static class GameExtensions
{
    private static readonly string[] gameSizes = { "B", "KB", "MB", "GB" };

    public static string ReadableSize(this IGame game)
    {
        int type = 0;
            double bytesLeft = game.Size!.Value;
            while (bytesLeft >= 1024)
            {
                type++;
                bytesLeft /= 1024;
            }

        return $"{bytesLeft:0.00} {gameSizes[type]}";
    }

    public static List<Command> GetCommands(this IGame game) => game.Source.GetGameCommands(game);
}