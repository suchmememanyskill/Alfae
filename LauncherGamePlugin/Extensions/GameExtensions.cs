using LauncherGamePlugin.Interfaces;

namespace LauncherGamePlugin.Extensions;

public static class GameExtensions
{
    public static IInstalledGame? GetInstalledGame(this IGame game) => game as IInstalledGame;
    public static bool IsInstalled(this IGame game) => game.GetInstalledGame() != null;
    private static readonly string[] gameSizes = { "B", "KB", "MB", "GB" };
    public static string ReadableSize(this IGame game)
    {
        int type = 0;
            double bytesLeft = game.Size;
            while (bytesLeft >= 1024)
            {
                type++;
                bytesLeft /= 1024;
            }

        return $"{bytesLeft:0.00} {gameSizes[type]}";
    }
}